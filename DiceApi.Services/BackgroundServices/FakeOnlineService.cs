using DiceApi.Services.Common;
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
    public class FakeOnlineService : BackgroundService
    {
        private IHubContext<OnlineUsersHub> _onlineContext;

        public FakeOnlineService(IHubContext<OnlineUsersHub> hubContext)
        {
            _onlineContext = hubContext;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Random _random = new Random();

                const int minDelayMilliseconds = 2000; // Минимальная задержка между отправками (2 секунды)
                const int maxDelayMilliseconds = 8000; // Максимальная задержка между отправками (5 секунд)
                int minUserCount = 43; // Минимальное число пользователей
                int maxUserCount = 77; // Максимальное число пользователей
                const int maxDifference = 2; // Максимальная разница между числами
                FakeActiveHelper.FakeUserCount = 61;

                while (true)
                {
                    if (DateTime.Now.Hour > 2 && DateTime.Now.Hour < 8)
                    {
                        minUserCount = 64;
                        maxUserCount = 102;
                    }
                    else if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 15)
                    {
                        minUserCount = 150;
                        maxUserCount = 300;
                    }
                    else
                    {
                        minUserCount = 345;
                        maxUserCount = 650;
                    }

                    if (FakeActiveHelper.FakeUserCount >= maxUserCount)
                    {
                        FakeActiveHelper.FakeUserCount -= 1;
                    }

                    if (FakeActiveHelper.FakeUserCount <= minUserCount)
                    {
                        FakeActiveHelper.FakeUserCount += 1;
                    }

                    int disconnectedUsers = _random.Next(-1, 0);
                    FakeActiveHelper.FakeUserCount += disconnectedUsers;
                    await _onlineContext.Clients.All.SendAsync("UserDisconnected", FakeActiveHelper.FakeUserCount + FakeActiveHelper.UserCount);

                    int delayMilliseconds = _random.Next(minDelayMilliseconds, maxDelayMilliseconds + 1);
                    await Task.Delay(delayMilliseconds);

                    int connectedUsers = _random.Next(0, maxDifference);
                    FakeActiveHelper.FakeUserCount += connectedUsers;

                    await _onlineContext.Clients.All.SendAsync("UserConnected", FakeActiveHelper.FakeUserCount + FakeActiveHelper.UserCount);

                    delayMilliseconds = _random.Next(minDelayMilliseconds, maxDelayMilliseconds + 1);
                    await Task.Delay(delayMilliseconds);
                }
            }
            );
        }
    }
}
