using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Roulette;
using DiceApi.Services;
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

        [HttpPost("bet")]//
        public async Task<CreateRouletteBetResponce> Bet(CreateRouletteBetRequest request)
        {
            if (GameStates.IsRouletteGameRun)
            {
                return new CreateRouletteBetResponce()
                {
                    Succesful = false,
                    Message = "Дождитесь окончания игры"
                };
            }

            return await _rouletteService.BetRouletteGame(request);
        }

        [HttpPost("getActiveBets")]
        public async Task<RouletteActiveBets> GetActiveBets()
        {
            var res = await _rouletteService.GetRouletteActiveBets();

            res.Bets.AddRange(FakeActiveHelper.FakeRouletteActiveBet);

            return res;
        }

        [HttpPost("getLastGamesResults")]
        public async Task<List<RouletteGameResult>> GetLastGamesResults()
        {
            return await _rouletteService.GetLastRouletteGameResults();
        }
    }
}
