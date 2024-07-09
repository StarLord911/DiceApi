using DiceApi.Common;
using DiceApi.Data.Data.Payment;
using DiceApi.Data.Data.Winning;
using DiceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DiceApi.Controllers
{
    [Route("api/winning")]
    [ApiController]
    public class WinningController : ControllerBase
    {
        private readonly ICacheService _cacheService;

        public WinningController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpPost("getWinningsToDay")]
        public async Task<WinningStats> GetWinningsToDay()
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);


            return stats;
        }
    }
}
