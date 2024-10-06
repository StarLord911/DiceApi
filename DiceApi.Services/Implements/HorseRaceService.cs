using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.HorseRace;
using DiceApi.Data.Data.Dice;
using DiceApi.Data.Data.HorseGame;
using DiceApi.Data.Data.Roulette;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class HorseRaceService : IHorseRaceService
    {
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IWageringRepository _wageringRepository;
        private readonly ILogRepository _logRepository;

        private readonly IHubContext<HorseGameBetsHub> _hubContext;

        public HorseRaceService(IUserService userService,
            ICacheService cacheService,
            IWageringRepository wageringRepository,
            IHubContext<HorseGameBetsHub> hubContext,
            ILogRepository logRepository)
        {
            _userService = userService;
            _cacheService = cacheService;
            _wageringRepository = wageringRepository;
            _hubContext = hubContext;
            _logRepository = logRepository;
        }

        public async Task<CreateHorseBetResponce> BetHorceRace(CreateHorseBetRequest request)
        {
            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_HORSE_RACE_USERS);

            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            if (!settingsCache.RouletteGameActive)
            {
                return new CreateHorseBetResponce
                {
                    Message = "Игра отключена.",
                    Succes = false
                };
            }

            if (bettedUserIds != null && bettedUserIds.Contains(request.UserId))
            {
                return new CreateHorseBetResponce
                {
                    Message = "Вы уже сделали ставку.",
                    Succes = false
                };
            }

            var user = _userService.GetById(request.UserId);

            if (!user.IsActive)
            {
                return new CreateHorseBetResponce
                {
                    Message = "Вы не можете делать ставки.",
                    Succes = false
                };
            }


            var betSum = request.HorseBets.Sum(b => b.BetSum);

            if (user.Ballance < betSum)
            {
                return new CreateHorseBetResponce
                {
                    Message = "Недостаточно баланса.",
                    Succes = false
                };
            }

            if (request.HorseBets.Any(b => b.BetSum < 1))
            {
                return new CreateHorseBetResponce
                {
                    Message = "Минимальная ставка 1.",
                    Succes = false
                };
            }

            if (request.HorseBets.Any(b => b.BetSum > 1000))
            {
                return new CreateHorseBetResponce
                {
                    Message = "Максимальная ставка 1000.",
                    Succes = false
                };
            }

            var updatedBallance = user.Ballance - betSum;

            await _userService.UpdateUserBallance(user.Id, updatedBallance);

            await UpdateWageringAsync(request.UserId, betSum);

            if (bettedUserIds == null)
            {
                bettedUserIds = new List<long>() { user.Id };
                await _cacheService.WriteCache(CacheConstraints.BETTED_HORSE_RACE_USERS, bettedUserIds);
            }
            else
            {
                bettedUserIds.Add(user.Id);

                await _cacheService.UpdateCache(CacheConstraints.BETTED_HORSE_RACE_USERS, bettedUserIds);
            }

            await _cacheService.WriteCache(CacheConstraints.HORSE_RACE_USER_BET + user.Id, request);

            await _logRepository.LogGame($"GAME HORSE User {user.Id} set new bet in horses betSum: {betSum}");

            foreach (var bet in request.HorseBets)
            {
                var gameJson = JsonConvert.SerializeObject(new HorseRaceActiveBet() 
                {
                    UserName = user.Name,
                    BetSum = bet.BetSum,
                    Multiplayer = 8,
                    HorseColor = bet.HorseColor
                });

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", gameJson);
            }

            return new CreateHorseBetResponce
            {
                Message = "",
                Succes = true
            };
        }

        public async Task<HorseRaceActiveBets> GetHorseGameActiveBets()
        {
            var result = new HorseRaceActiveBets();

            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_HORSE_RACE_USERS);

            if (bettedUserIds == null)
            {
                return result;
            }

            foreach (var id in bettedUserIds)
            {
                var userBets = await _cacheService.ReadCache<CreateHorseBetRequest>(CacheConstraints.HORSE_RACE_USER_BET + id);
                var user = _userService.GetById(id);

                if (userBets != null)
                {
                    foreach (var bet in userBets.HorseBets)
                    {
                        result.Bets.Add(new HorseRaceActiveBet() 
                        { 
                            UserName = user.Name, BetSum = bet.BetSum, Multiplayer = 8, HorseColor = bet.HorseColor 
                        });
                    }
                }
            }

            return result;
        }

        public async Task<List<HorseGameResult>> GetLastHorseGameResults()
        {
            var res = await _cacheService.ReadCache<List<HorseGameResult>>(CacheConstraints.LAST_HORSE_GAMES);

            if (res == null)
            {
                res = new List<HorseGameResult>();
            }

            return res;
        }

        #region private

        private async Task UpdateWageringAsync(long userId, decimal sum)
        {
            var wagering = await _wageringRepository.GetActiveWageringByUserId(userId);

            if (wagering != null && wagering.IsActive)
            {
                await _wageringRepository.UpdatePlayed(userId, sum);

                if (wagering.Wagering < wagering.Played + sum)
                {
                    await _wageringRepository.DeactivateWagering(wagering.Id);
                }
            }
        }

        #endregion
    }
}
