﻿using DiceApi.Common;
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
using DiceApi.Services.Contracts;
using DiceApi.Data.ApiReqRes.HorseRace;
using Newtonsoft.Json;
using DiceApi.Data.Data.Roulette;
using DiceApi.Services.Common;
using DiceApi.DataAcces.Repositoryes.Game;
using DiceApi.Data.Data.Games;

namespace DiceApi.Services.BackgroundServices
{
    public class HorsesService : BackgroundService
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly ILastGamesService _lastGamesService;
        private readonly IHubContext<HorseGameEndGameHub> _hubContext;
        private readonly IHubContext<HorseGameBetsHub> _horseBetsHub;

        private readonly IHubContext<HorsesGameStartTaimerHub> _gameStartTaimerHub;

        private readonly ILogRepository _logRepository;
        private readonly IGamesRepository _gamesRepository;

        public HorsesService(ICacheService cacheService, IUserService userService, IHubContext<HorseGameEndGameHub> hubContext, ILastGamesService lastGamesService,
            ILogRepository logRepository, IHubContext<HorsesGameStartTaimerHub> gameStartTaimerHub, IHubContext<HorseGameBetsHub> horseBetsHub,
            IGamesRepository gamesRepository)
        {
            _cacheService = cacheService;
            _userService = userService;
            _hubContext = hubContext;
            _lastGamesService = lastGamesService;
            _logRepository = logRepository;
            _gameStartTaimerHub = gameStartTaimerHub;
            _horseBetsHub = horseBetsHub;
            _gamesRepository = gamesRepository;
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
                await _logRepository.LogGame($"Horse game win {color}");

                var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_HORSE_RACE_USERS);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", color);

                foreach (var game in FakeActiveHelper.FakeHorseRaceActiveBet)
                {
                    await AddLastGames(game.UserName, game.BetSum, game.BetSum * 8, color == game.HorseColor);
                }

                if (bettedUserIds == null)
                {
                    await UpdateLastHorseGames(color);
                    await Taimer();
                    return;
                }

                var allBets = await GetAllBetsFromCacheAsync(bettedUserIds);

                var calculatedBets = CalculateBetSums(allBets.SelectMany(b => b.HorseBets).ToList()).OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);

                //color = ChechAntiMinus(color, calculatedBets);

                await UpdateLastHorseGames(color);
                await ProccessBets(color, allBets);

                await _cacheService.DeleteCache(CacheConstraints.BETTED_HORSE_RACE_USERS);
                await _logRepository.LogGame("Finish horse job");

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
                var log = new StringBuilder();

                if (userBets != null)
                {
                    var user = _userService.GetById(userBets.UserId);
                    log.AppendLine($"User horses bets UserId {user.Id}");
                    var winBet = userBets.HorseBets.FirstOrDefault(b => b.HorseColor == color);

                    if (winBet != null)
                    {
                        var winSum = winBet.BetSum * 8;
                        allWinSums += winSum;

                        log.AppendLine($"User set winned horse color: {winBet.HorseColor} betSum: {winBet.BetSum} winSum: {winSum}");

                        await _userService.UpdateUserBallance(user.Id, user.Ballance + winSum);

                    }

                    foreach (var bet in userBets.HorseBets)
                    {
                        var winSum = bet.HorseColor == color 
                            ? bet.BetSum * 8 
                            : 0;

                        await AddLastGames(user.Name, bet.BetSum, bet.BetSum * 8, winSum != 0);

                        var game = new GameModel();
                        game.BetSum = bet.BetSum;
                        game.UserId = user.Id;
                        game.Win = winSum != 0;
                        game.CanWin = bet.BetSum * 8;
                        game.GameTime = DateTime.Now.GetMSKDateTime();
                        game.GameType = GameType.Horses;
                        await _gamesRepository.AddGame(game);

                    }

                    await _logRepository.LogGame(log.ToString());

                }
            }

            await UpdateWinningToDay(allWinSums);
        }

        private async Task AddLastGames(string userName, decimal betSum, decimal canWin, bool win)
        {
            await _lastGamesService.AddLastGames(userName, betSum, canWin, win, GameType.Horses);
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
            var settings = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            FakeActiveHelper.FakeHorseRaceActiveBet = new List<HorseRaceActiveBet>();
            for (int i = 40; i != 0; i--)
            {
                Thread.Sleep(1000);

                if (i == 30)
                {
                    GameStates.IsHorseGameRun = false;
                }

                var random = new Random();

                if (i < 37 && settings.HorseGameActive)
                {
                    if (random.Next(0, 2) == 0)
                    {
                        var iterCount = random.Next(1, 5);
                        for (int x = 0; x < iterCount; x++)
                        {
                            var nameInex = random.Next(0, FakeActiveHelper.FakeNames.Count);

                            var betSum = FakeActiveHelper.RandomDigits[random.Next(0, FakeActiveHelper.RandomDigits.Count)];

                            var bet = new HorseRaceActiveBet()
                            {
                                UserName = FakeActiveHelper.FakeNames[nameInex],
                                BetSum = betSum,
                                Multiplayer = 8,
                                HorseColor = GetWinnedHorseColor()
                            };

                            FakeActiveHelper.FakeHorseRaceActiveBet.Add(bet);

                            var gameJson = JsonConvert.SerializeObject(bet);


                            await _horseBetsHub.Clients.All.SendAsync("ReceiveMessage", gameJson);
                        }
                    }
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
