﻿using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.HorseRace;
using DiceApi.Data.Data.HorseGame;
using DiceApi.Services.Common;
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

        [Authorize]
        [HttpPost("bet")]
        public async Task<CreateHorseBetResponce> BetHorse(CreateHorseBetRequest createHorseBetRequest)
        {
            if (GameStates.IsHorseGameRun)
            {
                return new CreateHorseBetResponce
                {
                    Message = "Дождитесь окончания игры",
                    Succes = false
                };
            }

            return await _horseRaceService.BetHorceRace(createHorseBetRequest);
        }


        [HttpPost("getActiveBets")]
        public async Task<HorseRaceActiveBets> GetActiveBets()
        {
            var res = await _horseRaceService.GetHorseGameActiveBets();

            res.Bets.AddRange(FakeActiveHelper.FakeHorseRaceActiveBet);

            return res;
        }

        [HttpPost("getLastGamesResults")]
        public async Task<List<HorseGameResult>> GetLastHorseGameResults()
        {
            return await _horseRaceService.GetLastHorseGameResults();
        }
    }
}
