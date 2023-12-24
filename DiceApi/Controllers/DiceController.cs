using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Data.Dice;
using DiceApi.Hubs;
using DiceApi.Services;
using DiceApi.Services.Contracts;
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
    [EnableCors("AllowAll")]
    [ApiController]
    public class DiceController : ControllerBase
    {
        private readonly IDiceService _diceService;
        private readonly IUserService _userService;
        private IHubContext<NewGameHub> _hubContext;


        public DiceController(IDiceService diceService,
            IUserService userService,
            IHubContext<NewGameHub> hubContext)
        {
            _diceService = diceService;
            _userService = userService;

            _hubContext = hubContext;
            
        }

        //[Authorize]
        [HttpPost("start")]
        public async Task<DiceResponce> Start(DiceRequest request)
        {
            //HttpContext.Items[]
            var res = await _diceService.StartDice(request);
            var user = _userService.GetById(res.Item2.UserId);

            var apiModel = new GameApiModel
            {
                UserName = ReplaceAt(user.Name, 4, '*'),
                Sum = res.Item2.Sum,
                CanWinSum = res.Item2.CanWin,
                Multiplier = Math.Round(res.Item2.CanWin / res.Item2.Sum, 2),
                Win = res.Item2.Win,
                GameType = Data.GameType.DiceGame
            };

            var gameJson = JsonConvert.SerializeObject(apiModel);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", gameJson);

            return res.Item1;
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
            var games = await _diceService.GetLastGames();

            var res = new List<GameApiModel>();

            foreach (var game in games)
            {
                var user = _userService.GetById(game.UserId);

                var apiModel = new GameApiModel
                {
                    UserName = ReplaceAt(user.Name, 4, '*'),
                    Sum = game.Sum,
                    CanWinSum = game.CanWin,
                    Multiplier = Math.Round(game.CanWin / game.Sum, 2)
                };

                res.Add(apiModel);
            }

            return res;
        }

        private string ReplaceAt(string input, int index, char newChar)
        {
            if (index < 0 || index >= input.Length)
            {
                return input;
            }

            char[] chars = input.ToCharArray();
            for (int i = index; i < input.Length; i++)
            {
                chars[i] = newChar;
            }
            return new string(chars);
        }
    }
}
