using DiceApi.Attributes;
using DiceApi.Data.Data.Dice;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi.Controllers
{
    [Route("api/dice")]
    [EnableCors("AllowAll")]
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
            //HttpContext.Items[]
            return await _diceService.StartDice(request);
        }

        [Authorize]
        [HttpPost("getAllGamesByUserId")]
        public async Task<List<DiceGame>> GetAllGamesById(GetDiceGamesRequest request)
        {
            return await _diceService.GetAllDiceGamesByUserId(request.UserId);
        }

        [Authorize]
        [HttpPost("getLastGames")]
        public async Task<List<DiceGame>> GetLastGames()
        {
            return await _diceService.GetLastGames();
        }
    }
}
