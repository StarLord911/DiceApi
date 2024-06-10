using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.HorseRace;
using DiceApi.Data.Data.HorseGame;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DiceApi.Controllers
{
    [Route("api/horse")]
    [ApiController]
    public class HorseGameController : ControllerBase
    {
        private readonly IHorseRaceService _horseRaceService;

        public HorseGameController(IHorseRaceService horseRaceService)
        {
            _horseRaceService = horseRaceService;
        }

        [HttpPost("bet")]
        public async Task<string> BetHorse(CreateHorseBetRequest createHorseBetRequest)
        {
            if (GameStates.IsHorseGameRun)
            {
                return "GameRunned";
            }

            return await _horseRaceService.BetHorceRace(createHorseBetRequest);
        }

        [HttpPost("getActiveBets")]
        public async Task<HorseRaceActiveBets> GetActiveBets()
        {
            return await _horseRaceService.GetHorseGameActiveBets();
        }

        [HttpPost("getLastGamesResults")]
        public async Task<List<HorseGameResult>> GetLastHorseGameResults()
        {
            return await _horseRaceService.GetLastHorseGameResults();
        }
    }
}
