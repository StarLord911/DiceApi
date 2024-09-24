using DiceApi.Common;
using DiceApi.Data.Data.Winning;
using DiceApi.Services.Common;
using DiceApi.Services.SignalRHubs;
using MathNet.Numerics.Random;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceApi.Services.BackgroundServices
{
    public class FakeMinesGameService : BackgroundService
    {
        private IHubContext<LastGamesHub> _newGameContext;
        private ICacheService _cacheService;

        public FakeMinesGameService(IHubContext<LastGamesHub> hubContext, ICacheService cacheService)
        {
            _newGameContext = hubContext;
            _cacheService = cacheService;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                var _random = new MersenneTwister();

                while (true)
                {
                    try
                    {
                        var apiModel = FakeActiveHelper.GetMinesGameApiModel();
                        var gameJson = JsonConvert.SerializeObject(apiModel);

                        if (apiModel.Win)
                        {
                            await UpdateWinningToDay(Math.Round(apiModel.Sum * (apiModel.Multiplier / 4), 2));
                        }

                        if (DateTime.Now.Hour > 2 && DateTime.Now.Hour < 8)
                        {
                            await Task.Delay(new MersenneTwister().Next(500, 1000));

                        }
                        else if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 15)
                        {
                            await Task.Delay(new MersenneTwister().Next(400, 700));
                        }
                        else
                        {
                            await Task.Delay(new MersenneTwister().Next(300, 500));
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

    }
}
