using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Promocode;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            var user = _userService.GetById(request.UserId);

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

            if (promocode.IsRefferalPromocode && !user.OwnerId.HasValue)
            {
                await _userService.UpdateRefferalOwnerId(request.UserId, promocode.RefferalPromocodeOwnerId.Value);
            }

            if (user.TelegramUserId == null)
            {
                responce.Message = "Привяжите телеграм";
                responce.Successful = false;
                return responce;
            }


            var updatedBallance = user.Ballance += promocode.BallanceAdd;
            await _userService.UpdateUserBallance(request.UserId, updatedBallance);

            var promocodeActivation = new PrimocodeActivation()
            {
                UserId = request.UserId,
                Promocode = request.Promocode,
                ActivationDateTime = DateTime.UtcNow,
                Wager = promocode.Wagering,
                AddedBallance = promocode.BallanceAdd
            };

            var wager = await _wageringRepository.GetActiveWageringByUserId(request.UserId);

            if (wager != null)
            {
                if (!wager.IsActive)
                {
                    await _wageringRepository.ActivateWagering(wager.Id);
                }

                await _wageringRepository.UpdateWagering(request.UserId, wager.Wagering + (promocode.BallanceAdd * promocode.Wagering));
            }
            else
            {
                var wearing = new Wager
                {
                    UserId = request.UserId,
                    Wagering = promocode.BallanceAdd * promocode.Wagering,
                    Played = 0,
                    IsActive = true
                };

                await _wageringRepository.AddWearing(wearing);
            }

            responce.Message = $"Активирован промокод на {promocode.BallanceAdd} руб.";
            responce.Successful = true;
            responce.BallanceAdded = promocode.BallanceAdd;
            await _promocodeActivationHistory.AddPromocodeActivation(promocodeActivation);

            return responce;
        }

        public async Task<string> CreatePromocode(CreatePromocodeRequest request)
        {
            var promocode = new Promocode()
            {
                ActivationCount = request.ActivationCount,
                BallanceAdd = request.BallanceAdd,
                PromoCode = request.Promocode,
                IsActive = true,
                Wagering = request.Wagering,
                IsRefferalPromocode = request.IsRefferalPromocode,
                RefferalPromocodeOwnerId = request.RefferalPromocodeOwnerId
            };

            var promocodeContains = await _promocodeRepository.IsPromocodeContains(promocode.PromoCode);

            if (promocodeContains)
            {
                return "Такой промокод уже существует";
            }

            await _promocodeRepository.CreatePromocode(promocode);

            return "Промокод успешно создан";
        }

        public async Task<GenerateRefferalPromocodeResponce> CreateRefferalPromocode(GenerateRefferalPromocodeRequest request)
        {
            var promocode = new Promocode()
            {
                ActivationCount = 100,
                BallanceAdd = 10,
                PromoCode = request.Promocode,
                IsActive = true,
                Wagering = 10,
                IsRefferalPromocode = true,
                RefferalPromocodeOwnerId = request.UserId
            };

            var user = _userService.GetById(request.UserId);

            if (!UserRole.IsPromocoder(user.Role))
            {
                return new GenerateRefferalPromocodeResponce()
                {
                    Message = "У вас нет прав, обратитесь в поддержку",
                    Success = false
                };
            }

            if (!Regex.IsMatch(request.Promocode, @"^[a-zA-Z]+$") && (request.Promocode.Length < 4 || request.Promocode.Length > 10))
            {
                return new GenerateRefferalPromocodeResponce()
                {
                    Message = "Промокод должен состоять из англ. символов. Длина от 4 до 10 символов.",
                    Success = false
                };
            }

            var promocodeContains = await _promocodeRepository.IsPromocodeContains(promocode.PromoCode);

            if (promocodeContains)
            {
                return new GenerateRefferalPromocodeResponce()
                {
                    Message = "Такой промокод уже существует",
                    Success = false
                };
            }

            if (promocode.PromoCode.Length < 4)
            {
                return new GenerateRefferalPromocodeResponce()
                {
                    Message = "Минимальная длина промокода 5 символов",
                    Success = false
                };
            }

            var promocodeCount = await _promocodeRepository.GetActiveRefferalPromocodeCount(request.UserId);

            if (promocodeCount >= 10)
            {
                return new GenerateRefferalPromocodeResponce()
                {
                    Message = "У вас больше 10 активных промокодов",
                    Success = false
                };
            }

            await _promocodeRepository.CreatePromocode(promocode);

            return new GenerateRefferalPromocodeResponce()
            {
                Message = "Промокод успешно создан",
                Success = true
            };
        }

        public async Task<PaginatedList<PromocodeApiModel>> GetPromocodeByNameByLike(GetPromocodesByNameRequest request)
        {
            var promocodes = await _promocodeRepository.GetPromocodeByLike(request.Name);

            promocodes = promocodes.Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize).ToList();

            var result = new List<PromocodeApiModel>();

            foreach (var code in promocodes)
            {
                var activationCount = (await _promocodeActivationHistory.GetPromocodeActivates(code.PromoCode)).Count;

                result.Add(new PromocodeApiModel
                {
                    Name = code.PromoCode,
                    ActivatedCount = activationCount,
                    AllActivationCount = code.ActivationCount,
                    BallanceAdd = code.BallanceAdd,
                    Wagering = code.Wagering,
                    IsRefferalPromocode = code.IsRefferalPromocode,
                    RefferalPromocodeOwnerId = code.RefferalPromocodeOwnerId
                });
            }

            var totalItemCount = promocodes.Count;

            var totalPages = (int)Math.Ceiling((double)totalItemCount / request.Pagination.PageSize);

            return new PaginatedList<PromocodeApiModel>(result.OrderBy(p => p.AllActivationCount - p.ActivatedCount).ToList(), totalPages, request.Pagination.PageNumber);
        }

        public async Task<PaginatedList<PromocodeApiModel>> GetPromocodesByPagination(GetPromocodesByPaginationRequest request)
        {
            var promocodes = await _promocodeRepository.GetAllPromocodes();

            if (request.OnlyActivePromocodes)
            {
                promocodes = promocodes.Where(p => p.IsActive).ToList();
            }

            promocodes = promocodes.Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize).ToList();

            var result = new List<PromocodeApiModel>();

            foreach (var code in promocodes)
            {
                var activationCount = (await _promocodeActivationHistory.GetPromocodeActivates(code.PromoCode)).Count;

                result.Add(new PromocodeApiModel
                {
                    Name = code.PromoCode,
                    ActivatedCount = activationCount,
                    AllActivationCount = code.ActivationCount,
                    BallanceAdd = code.BallanceAdd,
                    Wagering = code.Wagering,
                    IsRefferalPromocode = code.IsRefferalPromocode,
                    RefferalPromocodeOwnerId = code.RefferalPromocodeOwnerId
                });
            }

            var totalItemCount = promocodes.Count;

            var totalPages = (int)Math.Ceiling((double)totalItemCount / request.Pagination.PageSize);

            return new PaginatedList<PromocodeApiModel>(result.OrderBy(p => p.AllActivationCount - p.ActivatedCount).ToList(), totalPages, request.Pagination.PageNumber);
        }

        public async Task<List<RefferalPromocode>> GetRefferalPromocodesByUserId(long userId)
        {
            var promocodes = await _promocodeRepository.GetRefferalPromocodesByUserId(userId);
            var result = new List<RefferalPromocode>();


            if (promocodes.Count == 0)
            {
                return result;
            }

            foreach (var promocode in promocodes)
            {
                var activatedCount = (await _promocodeActivationHistory.GetPromocodeActivates(promocode.PromoCode)).Count;

                result.Add(new RefferalPromocode()
                {
                    Promocode = promocode.PromoCode,
                    ActivatedCount = activatedCount,
                    ActivationCount = promocode.ActivationCount
                });
            }

            return result;
        }
    }
}
