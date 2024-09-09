using DiceApi.Common;
using DiceApi.Data.Data.Winning;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.SignalRHubs;
using FluentScheduler;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Jobs
{
    /// <summary>
    /// Джоба удяляет записи в кеше о выигрышах и победах сегодня
    /// </summary>
    public class DropWinningsJob : Registry
    {
        private readonly ICacheService _cacheService;

        public DropWinningsJob(ICacheService cacheService)
        {
            _cacheService = cacheService;

            Schedule(async () => { await Execute(); }).ToRunOnceAt(0, 00);

        }

        private async Task Execute()
       {
            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, new WinningStats { WinningToDay = 0, WithdrawalToDay = 0 });
        }
    }
}
