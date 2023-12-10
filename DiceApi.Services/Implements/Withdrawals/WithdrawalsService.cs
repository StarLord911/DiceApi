using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Payment;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class WithdrawalsService : IWithdrawalsService
    {
        private readonly IWageringRepository _wageringRepository;
        private readonly IUserService _userService;
        private readonly IWithdrawalsRepository _withdrawalsRepository;
        private readonly IPaymentAdapterService _paymentAdapterService;
        private readonly IPaymentService _paymentService;

        public WithdrawalsService(IWageringRepository wageringRepository,
            IUserService userService,
            IWithdrawalsRepository withdrawalsRepository,
            IPaymentAdapterService paymentAdapterService,
            IPaymentService paymentService)
        {
            _wageringRepository = wageringRepository;
            _userService = userService;
            _withdrawalsRepository = withdrawalsRepository;
            _paymentAdapterService = paymentAdapterService;
            _paymentService = paymentService;
        }

        public async Task<CreateWithdrawalResponce> CreateWithdrawalRequest(CreateWithdrawalRequest request)
        {
            var wagering = await _wageringRepository.GetActiveWageringByUserId(request.UserId);
            var user = _userService.GetById(request.UserId);
            var responce = new CreateWithdrawalResponce();


            if (wagering != null && wagering.IsActive)
            {
                responce.Succses = false;
                responce.Message = $"Нужно отыграть по промокоду {wagering.Wageringed - wagering.Played}";

                return responce;
            }

            if (user != null && !user.IsActive)
            {
                responce.Succses = false;
                responce.Message = $"Не нашли юзера";

                return responce;
            }

            if (user.Ballance < request.Amount)
            {
                responce.Succses = false;
                responce.Message = $"Нехватка баланса";

                return responce;
            }

            var payments = await _paymentService.GetPaymentsByUserId(request.UserId);

            if (payments.Any() && user.PaymentForWithdrawal > payments.Sum(p => p.Amount))
            {
                responce.Succses = false;
                responce.Message = $"Недостаточно депозита для создания заявки на вывод";

                return responce;
            }

            var withdrowal = new Withdrawal
            {
                UserId = request.UserId,
                Amount = request.Amount,
                CardNumber = request.CartNumber,
                CreateDate = DateTime.Now,
                Status = WithdrawalStatus.New
            };

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.Amount);

            await _withdrawalsRepository.AddWithdrawal(withdrowal);

            responce.Succses = true;
            responce.Message = $"Заявка на вывод принята";

            return responce;
        }

        public async Task DeactivateWithdrawal(int id)
        {
            await _withdrawalsRepository.DeactivateWithdrawal(id);
        }

        public async Task<List<Withdrawal>> GetAll()
        {
            return await _withdrawalsRepository.GetAllActive();
        }

        public async Task<List<Withdrawal>> GetAllActiveByUserId(long userId)
        {
            return await _withdrawalsRepository.GetAllActiveByUserId(userId);
        }

        public async Task<List<Withdrawal>> GetAllByUserId(long userId)
        {
            return await _withdrawalsRepository.GetAllActiveByUserId(userId);
        }

        public async Task<PaginatedList<Withdrawal>> GetPaginatedWithdrawals(GetPaymentWithdrawalsRequest request)
        {
            return await _withdrawalsRepository.GetPaginatedWithdrawals(request);
        }

        public async Task<WithdrawalStats> GetWithdrawalStats()
        {
            var result = new WithdrawalStats();
            var allWithdrawals = await _withdrawalsRepository.GetAll();
            var withdrawals = allWithdrawals.Where(w => w.Status == WithdrawalStatus.New);

            result.ToDay = withdrawals.Where(r => r.CreateDate.Date == DateTime.Today).Sum(w => w.Amount);

            result.ToWeek = withdrawals.Where(r => IsThisWeek(r.CreateDate)).Sum(w => w.Amount);

            result.ToMonth = withdrawals.Where(r => IsThisMonth(r.CreateDate)).Sum(w => w.Amount);

            result.AllDays = withdrawals.Sum(w => w.Amount);

            result.WithdrawalWaitSum = allWithdrawals.Where(w => w.Status == WithdrawalStatus.New).Sum(w => w.Amount);

            return result;

        }

        public async Task<decimal> GetWithdrawalWaitSum()
        {
            return await _withdrawalsRepository.GetWithdrawalWaitSum();
        }

        public async Task СonfirmWithdrawal(long id)
        {
            var withdrawal = await _withdrawalsRepository.GetById(id);
            await _paymentAdapterService.CreateWithdrawal(withdrawal.Amount, withdrawal.CardNumber);
            await _withdrawalsRepository.DeactivateWithdrawal(id);
        }

        private bool IsThisMonth(DateTime dateTime)
        {
            return dateTime.Month == DateTime.Now.Month && dateTime.Year == DateTime.Now.Year;
        }

        private bool IsThisWeek(DateTime dateTime)
        {
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

            int currentWeek = calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int targetWeek = calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            return currentWeek == targetWeek;
        }
    }
}
