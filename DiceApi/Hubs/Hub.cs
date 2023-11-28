using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static int _userCount;
        private static List<long> _users = new List<long>();


        public async Task UserConnected(long userId)
        {
            if (_users.Contains(userId))
            {
                await Clients.All.SendAsync("UserConnected", _users.Count);
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

            await Clients.All.SendAsync("UserDisconnected", _users.Count);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _users.RemoveAt(1);
            await Clients.All.SendAsync("UserDisconnected", _users.Count);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
