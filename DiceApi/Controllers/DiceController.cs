using DiceApi.Attributes;
using DiceApi.Data.Data.Dice;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
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
        public DiceController(IDiceService diceService)
        {
            _diceService = diceService;
        }

        [Authorize]
        [HttpPost("start")]
        public async Task<DiceResponce> Start(DiceRequest request)
        {
            return await _diceService.StartDice(request);
        }

        [Authorize]
        [HttpPost("getAllGames")]
        public async Task<List<DiceGame>> GetAllGames( )
        {
            return await _diceService.GetAllDiceGames();
        }

        [Authorize]
        [HttpPost("getAllGamesByUserId")]
        public async Task<List<DiceGame>> GetAllGamesById(GetDiceGamesRequest request)
        {
            return await _diceService.GetAllDiceGamesByUserId(request.UserId);
        }

        [Authorize]
        [HttpPost("getLastGames")]
        public async Task<List<DiceGame>> GetLastGames(GetDiceGamesRequest request)
        {
            return await _diceService.GetAllDiceGamesByUserId(request.UserId);
        }
    }
}
