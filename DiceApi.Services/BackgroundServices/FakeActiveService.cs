using DiceApi.Services.SignalRHubs;
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


        public FakeActiveService(IHubContext<LastGamesHub> hubContext)
        {
            _newGameContext = hubContext;
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

                    if (DateTime.Now.Hour > 2 && DateTime.Now.Hour < 8)
                    {
                        await Task.Delay(new Random().Next(500, 2000));

                    }
                    else if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 15)
                    {
                        await Task.Delay(new Random().Next(500, 1500));
                    }
                    else
                    {
                        await Task.Delay(new Random().Next(500, 900));
                    }

                    await _newGameContext.Clients.All.SendAsync("ReceiveMessage", gameJson);
                }
            }
          );
        }
    }
}
