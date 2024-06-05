using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Roulette;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers
{
    [Route("api/roulette")]
    [ApiController]
    public class RouletteController : ControllerBase
    {
        private readonly IRouletteService _rouletteService;

        public RouletteController(IRouletteService rouletteService)
        {
            _rouletteService = rouletteService;
        }

        [HttpPost("bet")]
        public async Task<string> Bet(CreateRouletteBetRequest request)
        {
            return await _rouletteService.BetRouletteGame(request);
        }

        [HttpPost("getActiveBets")]
        public async Task<RouletteActiveBets> GetActiveBets()
        {
            return await _rouletteService.GetRouletteActiveBets();
        }

        [HttpPost("getLastGamesResults")]
        public async Task<List<RouletteGameResult>> GetLastGamesResults()
        {
            return await _rouletteService.GetLastRouletteGameResults();
        }
    }
}
