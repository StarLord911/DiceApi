using DiceApi.Attributes;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Dice;
using DiceApi.Services;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi.Controllers
{
    [Route("api/dice")]
    [ApiController]
    public class DiceController : ControllerBase
    {
        private readonly IDiceService _diceService;
        private readonly ILastGamesService _lastGamesService;

        public DiceController(IDiceService diceService,
            ILastGamesService lastGamesService)
        {
            _diceService = diceService;

            _lastGamesService = lastGamesService;            
        }

        [Authorize]
        [HttpPost("start")]
        public async Task<DiceResponce> Start(DiceRequest request)
        {
            //HttpContext.Items[]
            return await _diceService.StartDice(request);
        }

        [Authorize]
        [HttpPost("getAllGamesByUserId")]
        public async Task<List<DiceGame>> GetAllGamesById(GetDiceGamesRequest request)
        {
            return await _diceService.GetAllDiceGamesByUserId(request.UserId);
        }

        [HttpPost("getLastGames")]
        public async Task<List<GameApiModel>> GetLastGames()
        {
            return await _lastGamesService.GetLastGames();

        }
    }
}
