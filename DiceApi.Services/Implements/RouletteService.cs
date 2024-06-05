using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Roulette;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
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
        private readonly IHubContext<RouletteBetsHub> _hubContext;

        public RouletteService(IUserService userService,
            ICacheService cacheService,
            IHubContext<RouletteBetsHub> hubContext)
        {
            _userService = userService;
            _cacheService = cacheService;
            _hubContext = hubContext;
        }

        public async Task<string> BetRouletteGame(CreateRouletteBetRequest request)
        {
            var user = _userService.GetById(request.UserId);

            var betSum = request.Bets.Sum(b => b.BetSum);

            if (user.Ballance < betSum)
            {
                return "Low ballance";
            }


            var updatedBallance = user.Ballance - betSum;

            await _userService.UpdateUserBallance(user.Id, updatedBallance);

            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_ROULETTE_USERS);

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

            var gameJson = JsonConvert.SerializeObject(new RouletteActiveBet() { UserName = user.Name, BetSum = betSum });

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", gameJson);

            return "Succesfull";
        }

        public async Task<List<RouletteGameResult>> GetLastRouletteGameResults()
        {
            return await _cacheService.ReadCache<List<RouletteGameResult>>(CacheConstraints.LAST_ROULETTE_GAMES);
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

                if (userBets != null)
                {
                    var betsSum = userBets.Bets.Sum(b => b.BetSum);

                    result.Bets.Add(new RouletteActiveBet() { UserName = user.Name, BetSum = betsSum});
                }
            }

            return result;
        }
    }
}
