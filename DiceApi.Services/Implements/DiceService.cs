using DiceApi.Data.Data.Dice;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Winning;
using DiceApi.Data.ApiReqRes;
using DiceApi.DataAcces.Repositoryes.Game;
using DiceApi.Data.Data.Games;

namespace DiceApi.Services.Implements
{
    public class DiceService : IDiceService
    {
        private readonly IUserService _userService;
        private readonly IWageringRepository _wageringRepository;
        private readonly ICacheService _cacheService;
        private readonly ILastGamesService _lastGamesService;
        private readonly ILogRepository _logRepository;
        private readonly IGamesRepository _gamesRepository;

        public DiceService(IUserService userService,
            ILogRepository logRepository,
            IWageringRepository wageringRepository,
            ICacheService cacheService,
            ILastGamesService lastGamesService,
            IGamesRepository gamesRepository)
        {
            _gamesRepository = gamesRepository;
            _userService = userService;
            _logRepository = logRepository;
            _wageringRepository = wageringRepository;
            _cacheService = cacheService;
            _lastGamesService = lastGamesService;
        }

        public async Task<DiceResponce> StartDice(DiceRequest request)
        {
            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            if (!settingsCache.DiceGameActive)
            {
                return new DiceResponce
                {
                    Info = "Игра отключена",
                    IsSucces = false
                };
            }

            // Проверка валидности запроса
            if (!ValidateDiceRequest(request))
            {
                return new DiceResponce { IsSucces = false };
            }

            var response = new DiceResponce();

            // Вычисление возможного выигрыша
            var win = Convert.ToDecimal((100.0 / request.Persent)) * request.Sum;
            var winSum = Math.Round(win, 2);
            var user = _userService.GetById(request.UserId);

            // Генерация случайного числа для определения выигрыша

            // Чтение и десериализация настроек из кеша
            var cache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            var isStreamer = UserRole.IsStreamer(user.Role);
            // Проверка возможности выигрыша на основе анти-минус логики
            if (cache.DiceGameWinningSettings.DiceAntiminusBallance > request.Sum || isStreamer)
            {
                var random = new Random().Next(1, 100);

                var streamerBonus = isStreamer
                    ? 10
                    : 0;

                var rand = isStreamer
                    ? random
                    : random + cache.DiceGameWinningSettings.DiceMinusPercent;

                if (request.Persent < 5)
                {
                    rand -= cache.DiceGameWinningSettings.DiceMinusPercent;
                }

                if (request.Persent + streamerBonus > rand)
                {
                    // Обработка выигрыша
                    await HandleWinAsync(request, winSum, user, cache);
                    response.IsSucces = true;
                    response.NewBallance = user.Ballance;
                }
                else
                {
                    // Обработка проигрыша
                    await HandleLossAsync(request, user, cache);
                    response.IsSucces = false;
                    response.NewBallance = user.Ballance;
                }
            }
            else
            {
                // Обработка проигрыша из-за нехватки анти-минус баланса
                await HandleLossAsync(request, user, cache);
                response.IsSucces = false;
                response.NewBallance = user.Ballance;
            }

            // Обновление кеша
            await UpdateGameCacheAsync(cache);

            // Создание записи новой игры в базе данных
            var diceGame = await CreateDiceGameEntryAsync(request, response.IsSucces, winSum);

            // Обновление информации по вейджерам пользователя
            await UpdateWageringAsync(request.UserId, request.Sum);

            // Логирование новой игры
            await _logRepository.LogInfo($"Add new dice game for user {user.Id} win sum {winSum}");

            // Обновление ежедневных выигрышей (если выигрыш)

            await UpdateLastGames(diceGame, user);

            return response;
        }

        private async Task UpdateLastGames(GameModel diceGame, User user)
        {
            await _lastGamesService.AddLastGames(user.Name, diceGame.BetSum, diceGame.CanWin, diceGame.Win, GameType.DiceGame);
        }

        // Метод для обработки выигрыша
        private async Task HandleWinAsync(DiceRequest request, decimal winSum, User user, Settings cache)
        {
            user.Ballance += (winSum - request.Sum);
            await _userService.UpdateUserBallance(request.UserId, user.Ballance);
            cache.DiceGameWinningSettings.DiceAntiminusBallance -= winSum;
        }

        // Метод для обработки проигрыша
        private async Task HandleLossAsync(DiceRequest request, User user, Settings cache)
        {
            user.Ballance -= request.Sum;
            await _userService.UpdateUserBallance(request.UserId, user.Ballance);
            cache.DiceGameWinningSettings.DiceAntiminusBallance += request.Sum;
        }

        // Метод для обновления кеша с настройками игры
        private async Task UpdateGameCacheAsync(Settings cache)
        {
            await _cacheService.UpdateCache(CacheConstraints.SETTINGS_KEY, cache);
        }

        // Метод для создания записи новой игры в базе данных
        private async Task<GameModel> CreateDiceGameEntryAsync(DiceRequest request, bool isWin, decimal winSum)
        {
            var diceGame = new GameModel
            {
                UserId = request.UserId,
                BetSum = request.Sum,
                CanWin = winSum,
                Win = isWin,
                GameTime = DateTime.UtcNow.GetMSKDateTime(),
                GameType = GameType.DiceGame
            };
            await _gamesRepository.AddGame(diceGame);
            return diceGame;
        }

        // Метод для обновления информации по вейджерам пользователя
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

       

        public async Task<List<DiceGame>> GetAllDiceGamesByUserId(long userId)
        {
            //TODO
            return Enumerable.Empty<DiceGame>().ToList();
        }

        private async Task UpdateWinningToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            stats.WinningToDay += amount;

            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
        }

        private bool ValidateDiceRequest(DiceRequest request)
        {
            var containsUser = _userService.GetById(request.UserId);

            if (request.Persent < 1 || request.Persent > 95 || containsUser == null || request.Sum > containsUser.Ballance)
            {
                return false;
            }

            return true;
        }
    }
}