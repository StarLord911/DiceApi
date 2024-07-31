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

        public async Task<string> BetRouletteGame(CreateRouletteBetRequest request)
        {
            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_ROULETTE_USERS);

            if (bettedUserIds.Contains(request.UserId))
            {
                return "Bet already exist";
            }

            var user = _userService.GetById(request.UserId);

            var betSum = request.Bets.Sum(b => b.BetSum);

            if (user.Ballance < betSum)
            {
                return "Low ballance";
            }

            var updatedBallance = user.Ballance - betSum;

            await UpdateWageringAsync(request.UserId, betSum);

            await _userService.UpdateUserBallance(user.Id, updatedBallance);

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

            await _cacheService.WriteCache(CacheConstraints.ROULETTE_USER_BET + user.Id, request);

            await NotifyNewBets(request, user.Name);

            return "Succesfull";
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
                    var multiplier = 0;

                    if (bet.BetNumber.HasValue)
                    {
                        multiplier = 18;
                    }
                    else if (bet.BetColor.IsNotNullOrEmpty() && (bet.BetColor == RoutletteConsts.RED || bet.BetColor == RoutletteConsts.BLACK))
                    {
                        multiplier = 2;
                    }
                    else if (bet.BetRange.IsNotNullOrEmpty() && (bet.BetRange == RoutletteConsts.FirstRange || bet.BetRange == RoutletteConsts.SecondRange))
                    {
                        multiplier = 2;
                    }

                    result.Bets.Add(new RouletteActiveBet() { UserName = user.Name, BetSum = bet.BetSum, Multiplayer = multiplier });
                }
            }

            return result;
        }

        private async Task NotifyNewBets(CreateRouletteBetRequest request, string username)
        {
            foreach (var bet in request.Bets)
            {
                var multiplier = 0;

                if (bet.BetNumber.HasValue)
                {
                    multiplier = 18;
                }
                else if (bet.BetColor.IsNotNullOrEmpty() && (bet.BetColor == RoutletteConsts.RED || bet.BetColor == RoutletteConsts.BLACK))
                {
                    multiplier = 2;
                }
                else if (bet.BetRange.IsNotNullOrEmpty() && (bet.BetRange == RoutletteConsts.FirstRange || bet.BetRange == RoutletteConsts.SecondRange))
                {
                    multiplier = 2;
                }

                var gameJson = JsonConvert.SerializeObject(new RouletteActiveBet() { UserName = username, BetSum = bet.BetSum, Multiplayer = multiplier });

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
