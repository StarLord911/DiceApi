using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Dice;
using DiceApi.Data.Data.Roulette;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public class RouletteService : IRouletteService
    {
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly ILogRepository _logRepository;
        private readonly IWageringRepository _wageringRepository;

        private readonly IHubContext<RouletteBetsHub> _hubContext;

        public RouletteService(IUserService userService,
            ICacheService cacheService,
            IWageringRepository wageringRepository,
            ILogRepository logRepository,
            IHubContext<RouletteBetsHub> hubContext)
        {
            _logRepository = logRepository;
            _userService = userService;
            _cacheService = cacheService;
            _wageringRepository = wageringRepository;
            _hubContext = hubContext;
        }

        public async Task<CreateRouletteBetResponce> BetRouletteGame(CreateRouletteBetRequest request)
        {
            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            if (!settingsCache.RouletteGameActive)
            {
                return new CreateRouletteBetResponce
                {
                    Message = "Игра отключена",
                    Succesful = false
                };
            }

            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_ROULETTE_USERS);

            if (bettedUserIds != null && bettedUserIds.Contains(request.UserId))
            {
                return new CreateRouletteBetResponce()
                {
                    Succesful = false,
                    Message = "Вы уже сделали ставку."
                };
            }

            var user = _userService.GetById(request.UserId);

            var betSum = request.Bets.Sum(b => b.BetSum);

            if (!user.IsActive)
            {
                return new CreateRouletteBetResponce()
                {
                    Succesful = false,
                    Message = "Вы заблокированы."
                };
            }

            if (user.Ballance < betSum)
            {
                return new CreateRouletteBetResponce()
                {
                    Succesful = false,
                    Message = "Низкий баланс."
                };
            }

            if (request.Bets.Any(b => b.BetSum < 1))
            {
                return new CreateRouletteBetResponce()
                {
                    Succesful = false,
                    Message = "Минимальная ставка 1."
                };
            }

            if (request.Bets.Any(b => b.BetSum > 5000))
            {
                return new CreateRouletteBetResponce()
                {
                    Succesful = false,
                    Message = "Максимальная ставка 5000."
                };
            }

            var updatedBallance = user.Ballance - betSum;

            if (bettedUserIds == null)
            {
                bettedUserIds = new List<long>() { user.Id };
                await _cacheService.WriteCache(CacheConstraints.BETTED_ROULETTE_USERS, bettedUserIds);
            }
            else
            {
                bettedUserIds.Add(user.Id);

                await _cacheService.DeleteCache(CacheConstraints.BETTED_ROULETTE_USERS);

                await _cacheService.WriteCache(CacheConstraints.BETTED_ROULETTE_USERS, bettedUserIds);
            }

            await _logRepository.LogInfo($"User {user.Id} set new bet in roulette betSum: {betSum}");

            await UpdateWageringAsync(request.UserId, betSum);

            await _userService.UpdateUserBallance(user.Id, updatedBallance);

            await _cacheService.WriteCache(CacheConstraints.ROULETTE_USER_BET + user.Id, request);

            await NotifyNewBets(request, user.Name);

            return new CreateRouletteBetResponce()
            {
                Succesful = true,
                Message = string.Empty
            };
        }

        public async Task<List<RouletteGameResult>> GetLastRouletteGameResults()
        {
            var res = await _cacheService.ReadCache<List<RouletteGameResult>>(CacheConstraints.LAST_ROULETTE_GAMES);

            if (res == null)
            {
                return new List<RouletteGameResult>();
            }

            return res;
        }

        public async Task<RouletteActiveBets> GetRouletteActiveBets()
        {
            var result = new RouletteActiveBets();

            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_ROULETTE_USERS);

            if (bettedUserIds == null)
            {
                return result;
            }

            foreach (var id in bettedUserIds)
            {
                var userBets = await _cacheService.ReadCache<CreateRouletteBetRequest>(CacheConstraints.ROULETTE_USER_BET + id);
                var user = _userService.GetById(id);

                foreach (var bet in userBets.Bets)
                {
                    bool isColorBet = false;
                    bool isRangeBet = false;

                    var multiplier = 0;

                    if (bet.BetNumber.HasValue)
                    {
                        multiplier = 18;
                        isColorBet = false;
                        isRangeBet = false;
                    }
                    else if (bet.BetColor.IsNotNullOrEmpty() && (bet.BetColor == RoutletteConsts.RED || bet.BetColor == RoutletteConsts.BLACK))
                    {
                        multiplier = 2;
                        isColorBet = true;
                        isRangeBet = false;
                    }
                    else if (bet.BetRange.IsNotNullOrEmpty() && (bet.BetRange == RoutletteConsts.FirstRange || bet.BetRange == RoutletteConsts.SecondRange))
                    {
                        multiplier = 2;
                        isColorBet = false;
                        isRangeBet = true;
                    }

                    result.Bets.Add(new RouletteActiveBet() 
                    {
                        UserName = user.Name,
                        BetSum = bet.BetSum,
                        Multiplayer = multiplier,
                        IsColorBet = isColorBet,
                        BetColor = bet.BetColor,
                        IsRange = isRangeBet, 
                        Range = isRangeBet ? bet.BetRange : null,
                        BetNumber = isColorBet ? bet.BetNumber : null
                    });
                }
            }

            return result;
        }

        private async Task NotifyNewBets(CreateRouletteBetRequest request, string username)
        {
            foreach (var bet in request.Bets)
            {
                bool isColorBet = false;
                bool isRangeBet = false;

                var multiplier = 0;

                if (bet.BetNumber.HasValue)
                {
                    multiplier = 18;
                    isColorBet = false;
                    isRangeBet = false;
                }
                else if (bet.BetColor.IsNotNullOrEmpty() && (bet.BetColor == RoutletteConsts.RED || bet.BetColor == RoutletteConsts.BLACK))
                {
                    multiplier = 2;
                    isColorBet = true;
                    isRangeBet = false;
                }
                else if (bet.BetRange.IsNotNullOrEmpty() && (bet.BetRange == RoutletteConsts.FirstRange || bet.BetRange == RoutletteConsts.SecondRange))
                {
                    multiplier = 2;
                    isColorBet = false;
                    isRangeBet = true;
                }

                var gameJson = JsonConvert.SerializeObject(new RouletteActiveBet() 
                { 
                    UserName = username, 
                    BetSum = bet.BetSum, 
                    Multiplayer = multiplier,
                    IsColorBet = isColorBet, 
                    BetColor = isColorBet ? bet.BetColor : null,
                    IsRange = isRangeBet,
                    Range = isRangeBet ? bet.BetRange : null,
                    BetNumber = !isColorBet ? bet.BetNumber : null
                });

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", gameJson);
            }
        }

        private async Task UpdateWageringAsync(long userId, decimal sum)
        {
            var wagering = await _wageringRepository.GetActiveWageringByUserId(userId);

            if (wagering != null)
            {
                await _wageringRepository.UpdatePlayed(userId, sum);

                if (wagering.Wagering < wagering.Played + sum)
                {
                    await _wageringRepository.DeactivateWagering(wagering.Id);
                }
            }
        }
    }
}
