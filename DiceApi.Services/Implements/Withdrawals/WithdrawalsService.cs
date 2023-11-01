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

        public WithdrawalsService(IWageringRepository wageringRepository,
            IUserService userService,
            IWithdrawalsRepository withdrawalsRepository)
        {
            _wageringRepository = wageringRepository;
            _userService = userService;
            _withdrawalsRepository = withdrawalsRepository;
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
                responce.Message = $"Cannot find user";

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

            await _userService.UpdateUserBallance(request.UserId, -request.Amount);

            await _withdrawalsRepository.AddWithdrawal(withdrowal);

            responce.Succses = true;
            responce.Message = $"Заявка на вывод принята";

            return responce;
        }
    }
}
