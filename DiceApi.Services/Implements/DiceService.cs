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

namespace DiceApi.Services.Implements
{
    public class DiceService : IDiceService
    {
        private readonly IUserService _userService;
        private readonly IDiceGamesRepository _diceGamesRepository;
        private readonly IWageringRepository _wageringRepository;
        private readonly ICacheService _cacheService;
        private readonly ILastGamesService _lastGamesService;
        private readonly ILogRepository _logRepository;

        public DiceService(IUserService userService,
            IDiceGamesRepository diceGamesRepository,
            ILogRepository logRepository,
            IWageringRepository wageringRepository,
            ICacheService cacheService,
            ILastGamesService lastGamesService)
        {
            _userService = userService;
            _diceGamesRepository = diceGamesRepository;
            _logRepository = logRepository;
            _wageringRepository = wageringRepository;
            _cacheService = cacheService;
            _lastGamesService = lastGamesService;
        }

        public async Task<DiceResponce> StartDice(DiceRequest request)
        {
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
            var random = new Random().Next(1, 100);

            // Чтение и десериализация настроек из кеша
            var cache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            // Проверка возможности выигрыша на основе анти-минус логики
            if (cache.DiceGameWinningSettings.DiceAntiminusBallance > request.Sum && !UserRole.IsStreamer(user.Role))
            {
                var streamerBonus = UserRole.IsStreamer(user.Role)
                    ? 10
                    : 0;

                var rand = UserRole.IsStreamer(user.Role)
                    ? random
                    : random + cache.DiceGameWinningSettings.DiceMinusPercent;

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
            if (response.IsSucces)
            {
                await UpdateWinningToDay(winSum);
            }

            await UpdateLastGames(diceGame, user);

            return response;
        }

        private async Task UpdateLastGames(DiceGame diceGame, User user)
        {
            await _lastGamesService.AddLastGames(user.Name, diceGame.Sum, diceGame.CanWin, diceGame.Win, GameType.DiceGame);
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
        private async Task<DiceGame> CreateDiceGameEntryAsync(DiceRequest request, bool isWin, decimal winSum)
        {
            var diceGame = new DiceGame
            {
                UserId = request.UserId,
                Persent = request.Persent,
                Sum = request.Sum,
                CanWin = winSum,
                Win = isWin,
                GameTime = DateTime.UtcNow
            };
            await _diceGamesRepository.Add(diceGame);
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

        public async Task<List<DiceGame>> GetLastGames()
        {
            return await _diceGamesRepository.GetLastGames();
        }

        public async Task<List<DiceGame>> GetAllDiceGamesByUserId(long userId)
        {
            return await _diceGamesRepository.GetByUserId(userId);
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