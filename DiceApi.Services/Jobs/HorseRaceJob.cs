using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.HorseGame;
using DiceApi.Data.Data.Roulette;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.SignalRHubs;
using FluentScheduler;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceApi.Services.Jobs
{
    public class HorseRaceJob : Registry
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IHubContext<NewGameHub> _newGameHub;
        private readonly IHubContext<HorseGameEndGameHub> _hubContext;
        private readonly IHubContext<GameStartTaimerHub> _gameStartTaimerHub;

        private readonly ILogRepository _logRepository;

        public HorseRaceJob(ICacheService cacheService, IUserService userService, IHubContext<HorseGameEndGameHub> hubContext, IHubContext<NewGameHub> newGameHub,
            ILogRepository logRepository, IHubContext<GameStartTaimerHub> gameStartTaimerHub)
        {
            _cacheService = cacheService;
            _userService = userService;
            _hubContext = hubContext;
            _newGameHub = newGameHub;
            _logRepository = logRepository;
            _gameStartTaimerHub = gameStartTaimerHub;

            Schedule(() => RaceRun().GetAwaiter().GetResult()).NonReentrant().ToRunEvery(0).Seconds();
        }

        public async Task RaceRun()
        {
            GameStates.IsHorseGameRun = true;
            var color = GetWinnedHorseColor();

            var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_HORSE_RACE_USERS);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", color);

            if (bettedUserIds == null)
            {
                await Taimer();
                return;
            }

            await UpdateLastHorseGames(color);
            await _logRepository.LogInfo($"Horse game win {color.ToString()}");

            foreach (var id in bettedUserIds)
            {
                var userBets = await _cacheService.ReadCache<CreateHorseBetRequest>(CacheConstraints.HORSE_RACE_USER_BET + id);

                var user = _userService.GetById(id);

                var winBet = userBets.HorseBets.FirstOrDefault(b => b.HorseColor == color);

                if (winBet != null)
                {
                    var winSum = winBet.BetSum * 8;

                    await _userService.UpdateUserBallance(user.Id, user.Ballance + winSum);
                }

                await _cacheService.DeleteCache(CacheConstraints.HORSE_RACE_USER_BET + id);
            }

            await _cacheService.DeleteCache(CacheConstraints.BETTED_HORSE_RACE_USERS);

            GameStates.IsHorseGameRun = false;
            await Taimer();
        }

        private async Task Taimer()
        {
            for (int i = 0; i < 40; i++)
            {
                Thread.Sleep(1000);
                await _gameStartTaimerHub.Clients.All.SendAsync("ReceiveMessage", new GameTypeTaimer { GameName = "Horses", Taimer = i });
            }
        }

        public async Task UpdateLastHorseGames(HorseColor horseColor)
        {
            var lastGames =  await _cacheService.ReadCache<List<HorseGameResult>>(CacheConstraints.LAST_HORSE_GAMES);

            if (lastGames == null)
            {
                lastGames = new List<HorseGameResult>();
            }

            if (lastGames.Count > 49)
            {
                lastGames.RemoveAt(lastGames.Count);

            }

            lastGames.Add(
                new HorseGameResult()
                {
                    WinnedHorseColor = horseColor
                });

            await _cacheService.DeleteCache(CacheConstraints.LAST_HORSE_GAMES);

            await _cacheService.WriteCache(CacheConstraints.LAST_HORSE_GAMES, lastGames);
        }


        private HorseColor GetWinnedHorseColor()
        {
            Random random = new Random();
            HorseColor[] horseColors = Enum.GetValues(typeof(HorseColor)).Cast<HorseColor>().ToArray();
            HorseColor winnedHorseColor = horseColors[random.Next(horseColors.Length)];
            return winnedHorseColor;
        }
    }
}
