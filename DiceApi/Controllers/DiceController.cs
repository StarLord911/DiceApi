﻿using DiceApi.Attributes;
using DiceApi.Data.Data.Dice;
using DiceApi.Services;
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
        private readonly IUserService _userService;

        public DiceController(IDiceService diceService,
            IUserService userService)
        {
            _diceService = diceService;
            _userService = userService;
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
        public async Task<List<DiceGameApi>> GetLastGames()
        {
            var games = await _diceService.GetLastGames();

            var res = new List<DiceGameApi>();

            foreach (var game in games)
            {
                var user = _userService.GetById(game.UserId);

                var apiModel = new DiceGameApi
                {
                    UserName = ReplaceAt(user.Name, 3, '*'),
                    Sum = game.Sum,
                    CanWinSum = game.CanWin,
                    Multiplier = Math.Round(game.CanWin / game.Sum, 2)
                };

                res.Add(apiModel);
            }

            return res;
        }

        public string ReplaceAt(string input, int index, char newChar)
        {
            if (startIndex < 0 || startIndex >= input.Length)
            {
                throw new ArgumentOutOfRangeException("startIndex", "Start index is out of range");
            }

            char[] chars = input.ToCharArray();
            for (int i = startIndex; i < input.Length; i++)
            {
                chars[i] = newChar;
            }
            return new string(chars);
        }
    }
}
