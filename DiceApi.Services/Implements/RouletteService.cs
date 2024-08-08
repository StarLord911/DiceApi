using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
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
        private readonly IWageringRepository _wageringRepository;

        private readonly IHubContext<RouletteBetsHub> _hubContext;

        public RouletteService(IUserService userService,
            ICacheService cacheService,
            IWageringRepository wageringRepository,
            IHubContext<RouletteBetsHub> hubContext)
        {
            _userService = userService;
            _cacheService = cacheService;
            _wageringRepository = wageringRepository;
            _hubContext = hubContext;
        }

        public async Task<CreateRouletteBetResponce> BetRouletteGame(CreateRouletteBetRequest request)
        {
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

            if (user.Ballance < betSum)
            {
                return new CreateRouletteBetResponce()
                {
                    Succesful = false,
                    Message = "Низкий баланс."
                };
            }

            var updatedBallance = user.Ballance - betSum;

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

                    var multiplier = 0;

                    if (bet.BetNumber.HasValue)
                    {
                        multiplier = 18;
                        isColorBet = false;
                    }
                    else if (bet.BetColor.IsNotNullOrEmpty() && (bet.BetColor == RoutletteConsts.RED || bet.BetColor == RoutletteConsts.BLACK))
                    {
                        multiplier = 2;
                        isColorBet = true;
                    }
                    else if (bet.BetRange.IsNotNullOrEmpty() && (bet.BetRange == RoutletteConsts.FirstRange || bet.BetRange == RoutletteConsts.SecondRange))
                    {
                        multiplier = 2;
                        isColorBet = true;
                    }

                    result.Bets.Add(new RouletteActiveBet() 
                    {
                        UserName = user.Name,
                        BetSum = bet.BetSum,
                        Multiplayer = multiplier,
                        IsColorBet = isColorBet,
                        BetColor = bet.BetColor,

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

                var multiplier = 0;

                if (bet.BetNumber.HasValue)
                {
                    multiplier = 18;
                    isColorBet = false;
                }
                else if (bet.BetColor.IsNotNullOrEmpty() && (bet.BetColor == RoutletteConsts.RED || bet.BetColor == RoutletteConsts.BLACK))
                {
                    multiplier = 2;
                    isColorBet = true;
                }
                else if (bet.BetRange.IsNotNullOrEmpty() && (bet.BetRange == RoutletteConsts.FirstRange || bet.BetRange == RoutletteConsts.SecondRange))
                {
                    multiplier = 2;
                    isColorBet = true;
                }

                var gameJson = JsonConvert.SerializeObject(new RouletteActiveBet() 
                { 
                    UserName = username, 
                    BetSum = bet.BetSum, 
                    Multiplayer = multiplier,
                    IsColorBet = isColorBet, 
                    BetColor = isColorBet ? bet.BetColor : null,
                    
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
