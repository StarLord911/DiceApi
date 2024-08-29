using DiceApi.Common;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Roulette;
using DiceApi.Data.Data.Winning;
using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiceApi.Services.Contracts;
using DiceApi.Services.Implements;

namespace DiceApi.Services.BackgroundServices
{
    public class RouleteService : BackgroundService
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IHubContext<RouletteEndGameHub> _hubContext;
        private readonly IHubContext<RouletteGameStartTaimerHub> _gameStartTaimerHub;
        private readonly ILogRepository _logRepository;
        private readonly ILastGamesService _lastGamesService;

        private readonly string RED = "Red";
        private readonly string BLACK = "Black";

       
        public RouleteService(ICacheService cacheService, IUserService userService, IHubContext<RouletteEndGameHub> hubContext,
            ILogRepository logRepository, IHubContext<RouletteGameStartTaimerHub> gameStartTaimerHub,
            ILastGamesService lastGamesService)
        {
            _cacheService = cacheService;
            _userService = userService;
            _hubContext = hubContext;
            _logRepository = logRepository;
            _lastGamesService = lastGamesService;

            _gameStartTaimerHub = gameStartTaimerHub;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    await RouleteRun();
                }
                catch (Exception ex)
                {
                    await _logRepository.LogError($"RouleteRunRunContinuously error {ex}");
                }
            }
        }

        //TODO добавить антиминус
        private async Task RouleteRun()
        {
            try
            {
                GameStates.IsRouletteGameRun = true;
                var randomValue = new Random().Next(0, 18);

                await _logRepository.LogInfo($"Roulette random value {randomValue}");

                var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_ROULETTE_USERS);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", randomValue);
                await UpdateLastGames(randomValue);

                if (bettedUserIds == null)
                {
                    await _logRepository.LogInfo($"Roulette bettedUserIds is null");

                    await Taimer();
                    return;
                }

                decimal allWinSums = 0;

                foreach (var id in bettedUserIds)
                {
                    var userBets = await _cacheService.ReadCache<CreateRouletteBetRequest>(CacheConstraints.ROULETTE_USER_BET + id);
                    var user = _userService.GetById(id);

                    var log = new StringBuilder();

                    if (userBets != null)
                    {
                        decimal winSum = 0;
                        log.AppendLine($"User roulette bets UserId {user.Id} random value = {randomValue}");

                        foreach (var bet in userBets.Bets)
                        {
                            var multiplier = 0;

                            if (bet.BetNumber == randomValue)
                            {
                                winSum += bet.BetSum * 18;
                                multiplier = 18;
                                log.AppendLine($"Bet number {bet.BetNumber} multiplier = 18");

                            }
                            else if (bet.BetColor.IsNotNullOrEmpty() && bet.BetColor == RoutletteConsts.RED && GetDroppedColor(randomValue) == RoutletteConsts.RED)
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                                log.AppendLine($"Bet color RED multiplier = 2");

                            }
                            else if (bet.BetColor.IsNotNullOrEmpty() && bet.BetColor == RoutletteConsts.BLACK && GetDroppedColor(randomValue) == RoutletteConsts.BLACK)
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                                log.AppendLine($"Bet color BLACK multiplier = 2");
                            }
                            else if (bet.BetRange.IsNotNullOrEmpty() && bet.BetRange == RoutletteConsts.FirstRange && (randomValue >= 1 && randomValue <= 9))
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                                log.AppendLine($"Bet select FirstRange multiplier = 2");
                            }
                            else if (bet.BetRange.IsNotNullOrEmpty() && bet.BetRange == RoutletteConsts.SecondRange && (randomValue >= 10 && randomValue <= 18))
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                                log.AppendLine($"Bet select SecondRange multiplier = 2");
                            }

                            allWinSums += winSum;

                            await AddLastGames(user.Name, bet.BetSum, bet.BetSum * GetMultyplayer(bet), multiplier != 0);
                        }

                        await _logRepository.LogInfo(log.ToString());

                        await _userService.UpdateUserBallance(user.Id, winSum + user.Ballance);
                    }

                    await _cacheService.DeleteCache(CacheConstraints.ROULETTE_USER_BET + id);
                }

                await UpdateWinningToDay(allWinSums);

                await _cacheService.DeleteCache(CacheConstraints.BETTED_ROULETTE_USERS);
                GameStates.IsRouletteGameRun = false;
                await _logRepository.LogInfo("Finish roulette job");
                await Taimer();
            }
            catch (Exception ex)
            {
                await _logRepository.LogException("Exception in roulette", ex);
            }

        }

        private int GetMultyplayer(RouletteBet bet)
        {
            if (bet.BetNumber.HasValue)
            {
                return 18;
            }
            else if (bet.BetColor.IsNotNullOrEmpty() && bet.BetColor == RoutletteConsts.RED)
            {
                return 2;
            }
            else if (bet.BetColor.IsNotNullOrEmpty() && bet.BetColor == RoutletteConsts.BLACK)
            {
                return 2;
            }
            else if (bet.BetRange.IsNotNullOrEmpty() && bet.BetRange == RoutletteConsts.FirstRange)
            {
                return 2;
            }
            else if (bet.BetRange.IsNotNullOrEmpty() && bet.BetRange == RoutletteConsts.SecondRange)
            {
                return 2;
            }

            return 0;
        }

        private async Task AddLastGames(string userName, decimal betSum, decimal canWin, bool win)
        {
            await _lastGamesService.AddLastGames(userName, betSum, canWin, win, GameType.Roulette);
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
                    GameStates.IsRouletteGameRun = false;
                }

                await _gameStartTaimerHub.Clients.All.SendAsync("ReceiveMessage", SerializationHelper.Serialize(new GameTypeTaimer { GameName = "Roulette", Taimer = i }));
            }
        }

        public async Task UpdateLastGames(int droppedNumner)
        {
            var lastGames = await _cacheService.ReadCache<List<RouletteGameResult>>(CacheConstraints.LAST_ROULETTE_GAMES);

            if (lastGames == null)
            {
                lastGames = new List<RouletteGameResult>();
            }

            if (lastGames.Count > 10)
            {
                lastGames.RemoveAt(0);

            }

            lastGames.Add(
                new RouletteGameResult()
                {
                    DroppedNumber = droppedNumner,
                    DroppedCollor = GetDroppedColor(droppedNumner)
                });

            await _cacheService.DeleteCache(CacheConstraints.LAST_ROULETTE_GAMES);

            await _cacheService.WriteCache(CacheConstraints.LAST_ROULETTE_GAMES, lastGames);
        }

        private GameApiModel GetNewGameApiModel(decimal betSum, string userName, int multiplier)
        {
            var apiModel = new GameApiModel
            {
                UserName = ReplaceAt(userName, 4, '*'),
                Sum = betSum,
                CanWinSum = betSum * 2,
                Multiplier = multiplier,
                Win = multiplier != 0,
                GameType = Data.GameType.Roulette
            };

            return apiModel;
        }

        private string GetDroppedColor(int number)
        {
            if (number == 0)
            {
                return "Green";
            }

            if (number % 2 == 0)
            {
                return RED;
            }

            return BLACK;
        }

        private string ReplaceAt(string input, int index, char newChar)
        {
            if (index < 0 || index >= input.Length)
            {
                return input;
            }

            char[] chars = input.ToCharArray();
            for (int i = index; i < input.Length; i++)
            {
                chars[i] = newChar;
            }
            return new string(chars);
        }
    }
}
