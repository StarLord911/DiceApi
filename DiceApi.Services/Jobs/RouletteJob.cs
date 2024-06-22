using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Roulette;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.SignalRHubs;
using FluentScheduler;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceApi.Services.Jobs
{
    public class RouletteJob : Registry
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IHubContext<RouletteEndGameHub> _hubContext;
        private readonly IHubContext<NewGameHub> _newGameHub;
        private readonly IHubContext<RouletteGameStartTaimerHub> _gameStartTaimerHub;
        private readonly ILogRepository _logRepository;

        private readonly string RED = "Red";
        private readonly string BLACK = "Black";

        private readonly List<string> canBets = new List<string> 
        {
            "0", "1", "2", "3", "4",
            "5", "6", "7", "8", "9",
            "10", "11", "12", "13", "14",
            "15", "16", "17", "18",
            "Green"

        };

        public RouletteJob(ICacheService cacheService, IUserService userService, IHubContext<RouletteEndGameHub> hubContext, IHubContext<NewGameHub> newGameHub,
            ILogRepository logRepository, IHubContext<RouletteGameStartTaimerHub> gameStartTaimerHub)
        {
            _cacheService = cacheService;
            _userService = userService;
            _hubContext = hubContext;
            _newGameHub = newGameHub;
            _logRepository = logRepository;

            Schedule(() => RouleteRun().GetAwaiter().GetResult()).NonReentrant().ToRunEvery(0).Seconds();
            _gameStartTaimerHub = gameStartTaimerHub;
        }

        //TODO добавить антиминус
        public async Task RouleteRun()
        {
            try
            {
                GameStates.IsRouletteGameRun = true;
                var randomValue = new Random().Next(0, 18);

                await _logRepository.LogInfo($"Roulette random value {randomValue}");

                var bettedUserIds = await _cacheService.ReadCache<List<long>>(CacheConstraints.BETTED_ROULETTE_USERS);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", randomValue);

                if (bettedUserIds == null)
                {
                    await Taimer();
                    return;
                }

                foreach (var id in bettedUserIds)
                {
                    var userBets = await _cacheService.ReadCache<CreateRouletteBetRequest>(CacheConstraints.ROULETTE_USER_BET + id);
                    var user = _userService.GetById(id);

                    if (userBets != null)
                    {
                        decimal winSum = 0;

                        foreach (var bet in userBets.Bets)
                        {
                            var multiplier = 0;

                            if (bet.BetNumber == randomValue)
                            {
                                winSum += bet.BetSum * 18;
                                multiplier = 18;
                            }
                            else if (bet.BetColor.IsNotNullOrEmpty() && bet.BetColor == RoutletteConsts.RED && GetDroppedColor(randomValue) == RoutletteConsts.RED)
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                            }
                            else if (bet.BetColor.IsNotNullOrEmpty() && bet.BetColor == RoutletteConsts.BLACK && GetDroppedColor(randomValue) == RoutletteConsts.BLACK)
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                            }
                            else if(bet.BetRange.IsNotNullOrEmpty() && bet.BetRange == RoutletteConsts.FirstRange && (randomValue >= 1 && randomValue <= 9))
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                            }
                            else if (bet.BetRange.IsNotNullOrEmpty() && bet.BetRange == RoutletteConsts.SecondRange && (randomValue >= 10 && randomValue <= 18))
                            {
                                winSum += bet.BetSum * 2;
                                multiplier = 2;
                            }

                            var jsonGame = JsonConvert.SerializeObject(GetNewGameApiModel(bet.BetSum, user.Name, multiplier));

                            await _newGameHub.Clients.All.SendAsync(jsonGame);
                        }

                        await _userService.UpdateUserBallance(user.Id, winSum + user.Ballance);
                    }

                    await _cacheService.DeleteCache(CacheConstraints.ROULETTE_USER_BET + id);
                }

                await UpdateLastGames(randomValue);
                await _cacheService.DeleteCache(CacheConstraints.BETTED_ROULETTE_USERS);
                GameStates.IsRouletteGameRun = false;
                await Taimer();
            }
            catch (Exception ex)
            {
                await _logRepository.LogError("Exception in roulette" + ex.Message + ex.StackTrace);
                await _logRepository.LogException(ex);
            }
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
                await _logRepository.LogInfo("Taimer in roulette" + i);

                await _gameStartTaimerHub.Clients.All.SendAsync("ReceiveMessage", SerializationHelper.Serialize(new GameTypeTaimer { GameName = "Roulette", Taimer = i}));
            }
        }

        public async Task UpdateLastGames(int droppedNumner)
        {
            var lastGames = await _cacheService.ReadCache<List<RouletteGameResult>>(CacheConstraints.LAST_ROULETTE_GAMES);

            if (lastGames == null)
            {
                lastGames = new List<RouletteGameResult>();
            }

            if (lastGames.Count > 49)
            {
                lastGames.RemoveAt(lastGames.Count);

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
