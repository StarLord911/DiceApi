using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DiceApi.Services.SignalRHubs
{
    /// <summary>
    /// Этот класс преднозначен для отправки юзерам уведомления о том какое число упало на рулетке
    /// </summary>
    public class RouletteEndGameHub : Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Receive", message);
        }
    }

    /// <summary>
    /// Этот класс преднозначен для бокового меню с количеством ставок.
    /// </summary>
    public class RouletteBetsHub : Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Receive", message);
        }
    }

    public class HorseGameEndGameHub : Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Receive", message);
        }
    }
}
