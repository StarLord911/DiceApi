using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceApi.Hubs
{
    public class NewGameHub : Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Receive", message);
        }
    }

    public class OnlineUsersHub : Hub
    {
        private static int _fakeUsers;
        private static List<long> _users = new List<long>();
        private static Random _random = new Random();

        public OnlineUsersHub()
        {
            _fakeUsers = GetFakeUserCount();


        }

        public async Task SendRandomUserEvents()
        {
            const int minDelayMilliseconds = 2000; // Минимальная задержка между отправками (2 секунды)
            const int maxDelayMilliseconds = 5000; // Максимальная задержка между отправками (5 секунд)
            const int minUserCount = 250; // Минимальное число пользователей
            const int maxUserCount = 300; // Максимальное число пользователей
            const int maxDifference = 2; // Максимальная разница между числами

            while (true)
            {
                // Отправляем UserDisconnected
                int disconnectedUsers = _random.Next(minUserCount, maxUserCount + 1);
                await UserDisconnected(disconnectedUsers);

                // Задержка перед следующей отправкой
                int delayMilliseconds = _random.Next(minDelayMilliseconds, maxDelayMilliseconds + 1);
                await Task.Delay(delayMilliseconds);

                // Отправляем UserConnected
                int connectedUsers = disconnectedUsers + _random.Next(-maxDifference, maxDifference + 1);
                await UserConnected(connectedUsers);

                // Задержка перед следующей отправкой
                delayMilliseconds = _random.Next(minDelayMilliseconds, maxDelayMilliseconds + 1);
                await Task.Delay(delayMilliseconds);
            }
        }

        public async Task UserConnected(long userId)
        {
            if (_users.Contains(userId))
            {
                await Clients.All.SendAsync("UserConnected", _users.Count + _fakeUsers);
                return;
            }

            _users.Add(userId);

            await Clients.All.SendAsync("UserConnected", _users.Count);
        }

        public async Task UserDisconnected(long userId)
        {
            if (_users.Contains(userId))
            {
                _users.Remove(userId);
            }

            await Clients.All.SendAsync("UserDisconnected", _users.Count + _fakeUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _users.RemoveAt(1);
            await Clients.All.SendAsync("UserDisconnected", _users.Count + _fakeUsers);

            await base.OnDisconnectedAsync(exception);
        }

        private int GetFakeUserCount()
        {
            var randomUsers = _random.Next(1, 3);
            return 118 + randomUsers;
        }
    }
}