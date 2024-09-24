using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Winning;
using DiceApi.Services.Common;
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
                MersenneTwister _random = new MersenneTwister();

                while (true)
                {
                    try
                    {
                        var apiModel = FakeActiveHelper.GetDiceGameApiModel();
                        var gameJson = JsonConvert.SerializeObject(apiModel);

                        if (apiModel.Win)
                        {
                            await UpdateWinningToDay(Math.Round(apiModel.Sum * (apiModel.Multiplier / 4), 2));
                        }

                        if (DateTime.Now.Hour > 2 && DateTime.Now.Hour < 8)
                        {
                            await Task.Delay(new MersenneTwister().Next(1500, 500));

                        }
                        else if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 15)
                        {
                            await Task.Delay(new MersenneTwister().Next(150, 700));
                        }
                        else
                        {
                            await Task.Delay(new MersenneTwister().Next(150, 300));
                        }

                        if (_minutes.Contains(DateTime.Now.Minute))
                        {
                            var randoom = new MersenneTwister();
                            await UpdateWithdrawalToDay(randoom.Next(5000, 10000) + randoom.NextDecimal());
                        }

                        await _newGameContext.Clients.All.SendAsync("ReceiveMessage", gameJson);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });

            
        }

        private async Task UpdateWinningToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            stats.WinningToDay += Math.Round(amount);

            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
        }

        private async Task UpdateWithdrawalToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            if ((stats.WinningToDay - (stats.WithdrawalToDay + amount)) > 354651)
            {
                stats.WithdrawalToDay += Math.Round(amount, 2);

                await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
            }
        }
    }
}
