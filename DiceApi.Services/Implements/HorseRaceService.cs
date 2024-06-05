using DiceApi.Common;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.HorseRace;
using DiceApi.Data.Data.HorseGame;
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

namespace DiceApi.Services.Implements
{
    public class HorseRaceService : IHorseRaceService
    {
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IHubContext<RouletteBetsHub> _hubContext;

        public HorseRaceService(IUserService userService,
            ICacheService cacheService,
            IHubContext<RouletteBetsHub> hubContext)
        {
            _userService = userService;
            _cacheService = cacheService;
            _hubContext = hubContext;
        }

        public async Task<string> BetHorceRace(CreateHorseBetRequest request)
        {
            var user = _userService.GetById(request.UserId);

            var betSum = request.HorseBets.Sum(b => b.BetSum);

            if (user.Ballance < betSum)
            {
                return "Low ballance";
            }

            var updatedBallance = user.Ballance - betSum;

            await _userService.UpdateUserBallance(user.Id, updatedBallance);

            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_HORSE_RACE_USERS);

            if (bettedUserIds == null)
            {
                bettedUserIds = new List<long>() { user.Id };
                await _cacheService.WriteCache(CacheConstraints.BETTED_HORSE_RACE_USERS, bettedUserIds);
            }
            else
            {
                bettedUserIds.Add(user.Id);

                await _cacheService.DeleteCache(CacheConstraints.BETTED_HORSE_RACE_USERS);

                await _cacheService.WriteCache(CacheConstraints.BETTED_HORSE_RACE_USERS, bettedUserIds);
            }

            await _cacheService.WriteCache(CacheConstraints.HORSE_RACE_USER_BET + user.Id, request);

            var gameJson = JsonConvert.SerializeObject(new HorseRaceActiveBet() { UserName = user.Name, BetSum = betSum });

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", gameJson);

            return "Succesfull";
        }

        public async Task<HorseRaceActiveBets> GetHorseGameActiveBets()
        {
            var result = new HorseRaceActiveBets();

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

                    result.Bets.Add(new HorseRaceActiveBet() { UserName = user.Name, BetSum = betsSum });
                }
            }

            return result;
        }

        public async Task<List<HorseGameResult>> GetLastHorseGameResults()
        {
            return await _cacheService.ReadCache<List<HorseGameResult>>(CacheConstraints.LAST_HORSE_GAMES);
        }
    }
}
