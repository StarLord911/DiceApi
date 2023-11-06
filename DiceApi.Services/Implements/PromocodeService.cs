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
    public class PromocodeService : IPromocodeService
    {
        private readonly IUserService _userService;
        private readonly IPromocodeActivationHistory _promocodeActivationHistory;
        private readonly IPromocodeRepository _promocodeRepository;

        private readonly IWageringRepository _wageringRepository;

        public PromocodeService(IPromocodeRepository promocodeRepository,
            IUserService userService,
            IPromocodeActivationHistory promocodeActivationHistory,
            IWageringRepository wageringRepository)
        {
            _promocodeRepository = promocodeRepository;
            _userService = userService;
            _promocodeActivationHistory = promocodeActivationHistory;
            _wageringRepository = wageringRepository;
        }

        public async Task<ActivatePromocodeResponce> ActivetePromocode(ActivatePromocodeRequest request)
        {
            var promocode = await _promocodeRepository.GetPromocode(request.Promocode);
            var responce = new ActivatePromocodeResponce();

            if (promocode == null)
            {
                responce.Message = "Неверный промокод";
                responce.Successful = false;
                return responce;
            }

            var activateHistory = await _promocodeActivationHistory.GetPromocodeActivates(promocode.PromoCode);

            if (!promocode.IsActive)
            {
                responce.Message = "Неверный промокод";
                responce.Successful = false;
                return responce;
            }

            if (activateHistory.Count >= promocode.ActivationCount)
            {
                await _promocodeRepository.DiactivatePromocode(request.Promocode);
                responce.Message = "Превышено количество активаций промокода";
                responce.Successful = false;
                return responce;
            }

            if (activateHistory.Any(p => p.Promocode == request.Promocode && p.UserId == request.UserId))
            {
                responce.Message = "Вы уже активировали этот промокод";
                responce.Successful = false;
                return responce;
            }

            var user = _userService.GetById(request.UserId);

            var updatedBallance = user.Ballance += promocode.BallanceAdd;
            await _userService.UpdateUserBallance(request.UserId, updatedBallance);

            var promocodeActivation = new PrimocodeActivation()
            {
                UserId = request.UserId,
                Promocode = request.Promocode
            };

            var wager = await _wageringRepository.GetActiveWageringByUserId(request.UserId);

            if (wager != null)
            {
                if (!wager.IsActive)
                {
                    await _wageringRepository.ActivateWagering(wager.Id);
                }

                await _wageringRepository.UpdateWagering(request.UserId ,promocode.BallanceAdd * promocode.Wagering);
            }
            else
            {
                var wearing = new Wagering
                {
                    UserId = request.UserId,
                    Wageringed = promocode.BallanceAdd * promocode.Wagering,
                    Played = 0,
                    IsActive = true
                };

                await _wageringRepository.AddWearing(wearing);
            }

            responce.Message = "Промокод активирован";
            responce.Successful = true;
            await _promocodeActivationHistory.AddPromocodeActivation(promocodeActivation);

            return responce;
        }

        public async Task<Promocode> CreatePromocode(CreatePromocodeRequest request)
        {
            var promocode = new Promocode()
            {
                ActivationCount = request.ActivationCount,
                BallanceAdd = request.BallanceAdd,
                PromoCode = request.Promocode,
                IsActive = true,
                Wagering = request.Wagering
            };

            var promocodeContains = await _promocodeRepository.IsPromocodeContains(promocode.PromoCode);

            if (promocodeContains)
            {
                throw new Exception("Is promocode already contains");
            }

            await _promocodeRepository.CreatePromocode(promocode);

            return promocode;
        }
    }
}
