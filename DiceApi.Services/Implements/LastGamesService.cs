using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Winning;
using DiceApi.Services.Common;
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
    public class LastGamesService : ILastGamesService
    {
        private readonly ICacheService _cacheService;
        private IHubContext<LastGamesHub> _lastGamesHub;

        public LastGamesService(ICacheService cacheService,
            IHubContext<LastGamesHub> hubContext)
        {
            _cacheService = cacheService;
            _lastGamesHub = hubContext;
        }

        public async Task AddLastGames(string userName, decimal betSum, decimal canWin, bool win, GameType gameType)
        {
            var gameApiModel = new GameApiModel
            {
                UserName = ReplaceAt(userName, 4, '*'),
                Sum = betSum,
                CanWinSum = canWin,
                Multiplier = Math.Round(canWin / betSum, 2),
                Win = win,
                GameType = gameType,
                GameDate = DateTime.UtcNow.GetMSKDateTime().ToString("HH:mm")
            };

            if (win)
            {
                await UpdateWinningToDay(Math.Round(betSum * gameApiModel.Multiplier, 2));
            }

            var lastGames = await _cacheService.ReadCache<List<GameApiModel>>(CacheConstraints.LAST_GAMES_IN_SITE);

            var newLastGames = LastGamesInSiteHelper.UpdateLastGames(lastGames ?? new List<GameApiModel>(), gameApiModel);
            await _cacheService.UpdateCache(CacheConstraints.LAST_GAMES_IN_SITE, newLastGames);

            var gameJson = JsonConvert.SerializeObject(gameApiModel);

            await _lastGamesHub.Clients.All.SendAsync("ReceiveMessage", gameJson);
        }

        public async Task<List<GameApiModel>> GetLastGames()
        {
            var lastGames = await _cacheService.ReadCache<List<GameApiModel>>(CacheConstraints.LAST_GAMES_IN_SITE);

            if (lastGames == null)
            {
                return new List<GameApiModel>();
            }

            return lastGames;
        }

        public async Task SendNewLastGames(string userName, decimal betSum, decimal canWin, bool win, GameType gameType)
        {
            var gameApiModel = new GameApiModel
            {
                UserName = ReplaceAt(userName, 4, '*'),
                Sum = betSum,
                CanWinSum = canWin,
                Multiplier = Math.Round(canWin / betSum, 2),
                Win = win,
                GameType = gameType,
                GameDate = DateTime.Now.GetMSKDateTime().ToString("HH:mm")
            };

            var gameJson = JsonConvert.SerializeObject(gameApiModel);

            await _lastGamesHub.Clients.All.SendAsync("ReceiveMessage", gameJson);
        }

        private async Task UpdateWinningToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            stats.WinningToDay += amount;

            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
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
