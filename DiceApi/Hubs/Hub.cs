using DiceApi.Services;
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
        private static List<long> _users = new List<long>();
        private static Random _random = new Random();

        public OnlineUsersHub()
        {
        }


        public async Task UserConnected(long userId)
        {
            if (_users.Contains(userId))
            {
                await Clients.All.SendAsync("UserConnected", _users.Count + FakeActiveHelper.FakeUserCount);
                return;
            }
            _users.Add(userId);
            FakeActiveHelper.UserCount = _users.Count;

            await Clients.All.SendAsync("UserConnected", FakeActiveHelper.FakeUserCount + _users.Count);
        }

        public async Task UserDisconnected(long userId)
        {
            if (_users.Contains(userId))
            {
                _users.Remove(userId);
            }
            FakeActiveHelper.UserCount = _users.Count;

            await Clients.All.SendAsync("UserDisconnected", _users.Count + FakeActiveHelper.FakeUserCount);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _users.RemoveAt(1);
            await Clients.All.SendAsync("UserDisconnected", _users.Count + FakeActiveHelper.FakeUserCount);
            FakeActiveHelper.UserCount = _users.Count;

            await base.OnDisconnectedAsync(exception);
        }

       
    }
}