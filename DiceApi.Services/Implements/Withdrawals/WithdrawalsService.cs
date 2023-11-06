using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
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

        public WithdrawalsService(IWageringRepository wageringRepository,
            IUserService userService,
            IWithdrawalsRepository withdrawalsRepository,
            IPaymentAdapterService paymentAdapterService)
        {
            _wageringRepository = wageringRepository;
            _userService = userService;
            _withdrawalsRepository = withdrawalsRepository;
            _paymentAdapterService = paymentAdapterService;
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

            var withdrowal = new Withdrawal
            {
                UserId = request.UserId,
                Amount = request.Amount,
                CardNumber = request.CartNumber,
                CreateDate = DateTime.Now,
                IsActive = true
            };

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.Amount);

            await _withdrawalsRepository.AddWithdrawal(withdrowal);

            responce.Succses = true;
            responce.Message = $"Заявка на вывод принята";

            return responce;
        }

        public async Task<List<Withdrawal>> GetAll()
        {
            return await _withdrawalsRepository.GetAll();
        }

        public async Task<List<Withdrawal>> GetByUserId(long userId)
        {
            return await _withdrawalsRepository.GetAllByUserId(userId);
        }

        public async Task СonfirmWithdrawal(long id)
        {
            var withdrawal = await _withdrawalsRepository.GetById(id);
            await _paymentAdapterService.CreateWithdrawal(withdrawal.Amount, withdrawal.CardNumber);
            await _withdrawalsRepository.UpdateIsActive(id);
        }
    }
}
