using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Payment;
using DiceApi.Data.Data.Payment.FreeKassa;
using DiceApi.Data.Data.Winning;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiceApi.Services.Implements
{
    public class WithdrawalsService : IWithdrawalsService
    {
        private readonly IWageringRepository _wageringRepository;
        private readonly IUserService _userService;
        private readonly IWithdrawalsRepository _withdrawalsRepository;
        private readonly IPaymentAdapterService _paymentAdapterService;
        private readonly IPaymentService _paymentService;
        private readonly ICacheService _cacheService;
        private readonly ILogRepository _logRepository;

        public WithdrawalsService(IWageringRepository wageringRepository,
            IUserService userService,
            IWithdrawalsRepository withdrawalsRepository,
            IPaymentAdapterService paymentAdapterService,
            IPaymentService paymentService,
            ICacheService cacheService,
            ILogRepository logRepository)
        {
            _wageringRepository = wageringRepository;
            _userService = userService;
            _withdrawalsRepository = withdrawalsRepository;
            _paymentAdapterService = paymentAdapterService;
            _paymentService = paymentService;
            _cacheService = cacheService;
            _logRepository = logRepository;
        }

        public async Task<CreateWithdrawalResponce> CreateWithdrawalRequest(CreateWithdrawalRequest request)
        {
            var wagering = await _wageringRepository.GetActiveWageringByUserId(request.UserId);
            var user = _userService.GetById(request.UserId);
            var responce = new CreateWithdrawalResponce();

            var cache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            if (!cache.WithdrawalActive)
            {
                responce.Succses = false;
                responce.Message = $"Выводы отключены";

                return responce;
            }

            if (wagering != null && wagering.IsActive && (wagering.Wagering - wagering.Played > 0))
            {
                responce.Succses = false;
                responce.Message = $"Нужно отыграть по промокоду {wagering.Wagering - wagering.Played}";

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

            if (!user.EnableWithdrawal)
            {
                responce.Succses = false;
                responce.Message = $"У данного пользователя отключены выводы";

                return responce;
            }

            if (request.Amount < 1029 && request.WithdrawalType == WithdrawalType.Sbp)
            {
                responce.Succses = false;
                responce.Message = $"Минимальная сумма вывода по СБП 1030";

                return responce;
            }

            if (request.Amount < 1999 && request.WithdrawalType == WithdrawalType.CardNumber)
            {
                responce.Succses = false;
                responce.Message = $"Минимальная сумма вывода по номеру карты 2000";

                return responce;
            }

            var withdrowal = new Withdrawal
            {
                UserId = request.UserId,
                Amount = request.Amount,
                CardNumber = request.CartNumber,
                CreateDate = DateTime.UtcNow.AddHours(3),
                Status = WithdrawalStatus.Moderation,
                BankId = request.BankId
            };

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.Amount);
            await _withdrawalsRepository.AddWithdrawal(withdrowal);

            await _logRepository.LogInfo($"Create Withdrawal for user {user.Id}, amount {withdrowal.Amount}");
            responce.Succses = true;
            responce.Message = $"Заявка на вывод принята";
            return responce;
        }

        public async Task DeactivateWithdrawal(long id)
        {
            await _withdrawalsRepository.DeactivateWithdrawal(id);

            var data = await _withdrawalsRepository.GetById(id);

            await _userService.AddUserBallance(data.UserId, data.Amount);

            await _logRepository.LogInfo($"Deactivate withdrawal for user {data.UserId}, amount {data.Amount}");

        }

        public async Task<List<Withdrawal>> GetAll()
        {
            return await _withdrawalsRepository.GetAllActive();
        }

        public async Task<List<Withdrawal>> GetAllActiveByUserId(long userId)
        {
            return await _withdrawalsRepository.GetAllByUserId(userId);
        }

        public async Task<List<Withdrawal>> GetAllByUserId(long userId)
        {
            return await _withdrawalsRepository.GetAllByUserId(userId);
        }

        public async Task<PaginatedList<Withdrawal>> GetPaginatedWithdrawals(GetPaymentWithdrawalsRequest request)
        {
            return await _withdrawalsRepository.GetPaginatedWithdrawals(request);
        }

        public async Task<WithdrawalStats> GetWithdrawalStats()
        {
            var result = new WithdrawalStats();
            var allWithdrawals = await _withdrawalsRepository.GetAll();
            var withdrawals = allWithdrawals.Where(w => w.Status == WithdrawalStatus.Moderation);

            result.ToDay = withdrawals.Where(r => r.CreateDate.Date == DateTime.Today).Sum(w => w.Amount);

            result.ToWeek = withdrawals.Where(r => IsThisWeek(r.CreateDate)).Sum(w => w.Amount);

            result.ToMonth = withdrawals.Where(r => IsThisMonth(r.CreateDate)).Sum(w => w.Amount);

            result.AllDays = withdrawals.Sum(w => w.Amount);

            result.WithdrawalWaitSum = allWithdrawals.Where(w => w.Status == WithdrawalStatus.Moderation).Sum(w => w.Amount);

            return result;

        }

        public async Task<decimal> GetWithdrawalWaitSum()
        {
            return await _withdrawalsRepository.GetWithdrawalWaitSum();
        }

        public async Task СonfirmWithdrawal(long id)
        {
            var withdrawal = await _withdrawalsRepository.GetById(id);

            var res = await _paymentAdapterService.CreateWithdrawal(withdrawal);

            await UpdateStatus(withdrawal.Id, WithdrawalStatus.AdapterHandle);

            await _withdrawalsRepository.UpdateFkWaletId(withdrawal.Id, res.Data.Id);
            await UpdateWithdrawalToDay(withdrawal.Amount);

            await _logRepository.LogInfo($"Сonfirm withdrawal for user {withdrawal.UserId}, amount {withdrawal.Amount}");
        }

        private async Task UpdateWithdrawalToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            stats.WithdrawalToDay += amount;

            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
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

        public async Task<List<Withdrawal>> GetAllConfirmedAsync()
        {
            return await _withdrawalsRepository.GetAllConfirmedAsync();
        }

        public async Task UpdateStatus(long id, WithdrawalStatus status)
        {
            await _withdrawalsRepository.UpdateStatus(id, status);
        }

        public async Task<Withdrawal> GetById(long id)
        {
            return await _withdrawalsRepository.GetById(id);
        }

        public async Task UpdateStatusWithFkValetId(long id, WithdrawalStatus status)
        {
            await _withdrawalsRepository.UpdateStatusWithFkValetId(id, status);
        }

        public async Task<long> GetWithdrawalIdByFkWaletId(long id)
        {
            return await _withdrawalsRepository.GetWithdrawalIdByFkWaletId(id);
        }
    }
}
