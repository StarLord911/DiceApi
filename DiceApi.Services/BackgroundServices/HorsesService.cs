using DiceApi.Common;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.HorseGame;
using DiceApi.Data.Data.Winning;
using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceApi.Services.BackgroundServices
{
    public class HorsesService : BackgroundService
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IHubContext<NewGameHub> _newGameHub;
        private readonly IHubContext<HorseGameEndGameHub> _hubContext;
        private readonly IHubContext<HorsesGameStartTaimerHub> _gameStartTaimerHub;

        private readonly ILogRepository _logRepository;

        public HorsesService(ICacheService cacheService, IUserService userService, IHubContext<HorseGameEndGameHub> hubContext, IHubContext<NewGameHub> newGameHub,
            ILogRepository logRepository, IHubContext<HorsesGameStartTaimerHub> gameStartTaimerHub)
        {
            _cacheService = cacheService;
            _userService = userService;
            _hubContext = hubContext;
            _newGameHub = newGameHub;
            _logRepository = logRepository;
            _gameStartTaimerHub = gameStartTaimerHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    await RaceRun();
                }
                catch (Exception ex)
                {
                    await _logRepository.LogError($"Horse game error {ex}");
                }
            }
        }

        public async Task RaceRun()
        {
            try
            {
                GameStates.IsHorseGameRun = true;
                var color = GetWinnedHorseColor();
                await _logRepository.LogInfo($"Horse game win {color}");

                var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_HORSE_RACE_USERS);

                if (bettedUserIds == null)
                {
                    await UpdateLastHorseGames(color);
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", color);

                    await Taimer();
                    return;
                }

                var allBets = await GetAllBetsFromCacheAsync(bettedUserIds);

                var calculatedBets = CalculateBetSums(allBets.SelectMany(b => b.HorseBets).ToList()).OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);

                //color = ChechAntiMinus(color, calculatedBets);

                await UpdateLastHorseGames(color);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", color);

                await ProccessBets(color, allBets);

                await _cacheService.DeleteCache(CacheConstraints.BETTED_HORSE_RACE_USERS);
                await _logRepository.LogInfo("Finish horse job");

                await Taimer();
            }
            catch (Exception ex)
            {
                await _logRepository.LogException("Exception in horse", ex);
            }
        }

        private async Task ProccessBets(HorseColor color, List<CreateHorseBetRequest> allBets)
        {
            decimal allWinSums = 0;

            foreach (var userBets in allBets)
            {
                if (userBets != null)
                {
                    var user = _userService.GetById(userBets.UserId);

                    var winBet = userBets.HorseBets.FirstOrDefault(b => b.HorseColor == color);

                    if (winBet != null)
                    {
                        var winSum = winBet.BetSum * 8;
                        allWinSums += winSum;

                        await _userService.UpdateUserBallance(user.Id, user.Ballance + winSum);
                    }
                }
            }

            await UpdateWinningToDay(allWinSums);
        }

        private HorseColor ChechAntiMinus(HorseColor horseColor, Dictionary<HorseColor, decimal> pairs)
        {
            if (pairs.ContainsKey(horseColor))
            {
                var isFavorit = pairs.Keys.FirstOrDefault() == horseColor;
                var horseColorBetsCount = pairs.Count;

                if (new Random().Next(0, 1) == 1)
                {
                    if (isFavorit && horseColorBetsCount > 1)
                    {
                        return pairs.ElementAt(horseColorBetsCount - 1).Key;
                    }

                    return GetWinnedHorseColor();
                }
            }

            return horseColor;
        }

        private async Task<List<CreateHorseBetRequest>> GetAllBetsFromCacheAsync(List<long> userIds)
        {
            var allBets = new List<CreateHorseBetRequest>();

            foreach (var userId in userIds)
            {
                var cacheKey = CacheConstraints.HORSE_RACE_USER_BET + userId;
                var userBets = await _cacheService.ReadCache<CreateHorseBetRequest>(cacheKey);

                if (userBets != null)
                {
                    allBets.Add(userBets);
                    await _cacheService.DeleteCache(cacheKey);
                }
            }

            return allBets;
        }

        private Dictionary<HorseColor, decimal> CalculateBetSums(List<HorseBet> allBets)
        {
            var betSums = new Dictionary<HorseColor, decimal>();

            foreach (var bet in allBets)
            {
                if (!betSums.ContainsKey(bet.HorseColor))
                {
                    betSums[bet.HorseColor] = 0;
                }

                betSums[bet.HorseColor] += bet.BetSum;
            }

            return betSums;
        }


        private async Task UpdateWinningToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            stats.WinningToDay += amount;

            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
        }

        private async Task Taimer()
        {
            for (int i = 40; i != 0; i--)
            {
                Thread.Sleep(1000);

                if (i == 30)
                {
                    GameStates.IsHorseGameRun = false;
                }

                await _gameStartTaimerHub.Clients.All.SendAsync("ReceiveMessage", SerializationHelper.Serialize(new GameTypeTaimer { GameName = "Horses", Taimer = i }));
            }
        }

        public async Task UpdateLastHorseGames(HorseColor horseColor)
        {
            var lastGames = await _cacheService.ReadCache<List<HorseGameResult>>(CacheConstraints.LAST_HORSE_GAMES);

            if (lastGames == null)
            {
                lastGames = new List<HorseGameResult>();
            }

            if (lastGames.Count > 25)
            {
                lastGames.RemoveAt(lastGames.Count-1);
            }

            lastGames.Insert(0, new HorseGameResult()
            {
                WinnedHorseColor = horseColor
            });

            await _cacheService.UpdateCache(CacheConstraints.LAST_HORSE_GAMES, lastGames);
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
