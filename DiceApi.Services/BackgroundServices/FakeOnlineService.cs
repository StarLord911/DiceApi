using DiceApi.Common;
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
                    var hour = DateTime.Now.GetMSKDateTime().Hour;

                    if (hour > 2 && hour < 8)
                    {
                        minUserCount = 55;
                        maxUserCount = 70;
                    }
                    else if (hour > 8 && hour < 12)
                    {
                        minUserCount = 70;
                        maxUserCount = 100;
                    }
                    else if (hour > 12 && hour < 16)
                    {
                        minUserCount = 100;
                        maxUserCount = 125;
                    }
                    else if (hour > 16 && hour < 19)
                    {
                        minUserCount = 125;
                        maxUserCount = 180;
                    }
                    else
                    {
                        minUserCount = 180;
                        maxUserCount = 300;
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
