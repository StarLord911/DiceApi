using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Winning;
using DiceApi.Services.SignalRHubs;
using MathNet.Numerics;
using MathNet.Numerics.Random;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceApi.Services.BackgroundServices
{
    public class FakeActiveService : BackgroundService
    {
        private IHubContext<LastGamesHub> _newGameContext;
        private ICacheService _cacheService;

        private List<int> _minutes = new List<int>()
        {
            1,3,4,7,9,11,15,17,22,21,45,25,29,31,34,35,37,40,41,42,43,46,49,51,53,54,55,56,58,
        };

        public FakeActiveService(IHubContext<LastGamesHub> hubContext, ICacheService cacheService)
        {
            _newGameContext = hubContext;
            _cacheService = cacheService;

        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Random _random = new Random();

                while (true)
                {
                    var apiModel = FakeActiveHelper.GetGameApiModel();
                    var gameJson = JsonConvert.SerializeObject(apiModel);

                    if (apiModel.Win && _random.Next(0, 2) == 1)
                    {
                        await UpdateWinningToDay(Math.Round(apiModel.Sum * apiModel.Multiplier, 2));
                    }

                    if (DateTime.Now.Hour > 2 && DateTime.Now.Hour < 8)
                    {
                        await Task.Delay(new Random().Next(1000, 2000));

                    }
                    else if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 15)
                    {
                        await Task.Delay(new Random().Next(1000, 2000));
                    }
                    else
                    {
                        await Task.Delay(new Random().Next(500, 1500));
                    }

                    if (_minutes.Contains(DateTime.Now.Minute) && (DateTime.Now.Second == 27 || DateTime.Now.Second == 45))
                    {
                        var randoom = new Random();
                        await UpdateWithdrawalToDay(randoom.Next(2000, 3000) + randoom.NextDecimal());
                    }

                    await _newGameContext.Clients.All.SendAsync("ReceiveMessage", gameJson);
                }
            }
          );
        }

        private async Task UpdateWinningToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            stats.WinningToDay += (amount / 2);

            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
        }

        private async Task UpdateWithdrawalToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            if (stats.WinningToDay > stats.WithdrawalToDay + amount)
            {
                stats.WithdrawalToDay += amount;

                await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
            }
        }
    }
}
