using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Requests;
using DiceApi.Services;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers
{
    [Route("api/mines")]
    [ApiController]
    public class MinesController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMinesService _minesService;
        private IHubContext<NewGameHub> _hubContext;

        public MinesController(IUserService userService,
            IHubContext<NewGameHub> hubContext,
            IMinesService minesService)
        {
            _userService = userService;
            _minesService = minesService;
            _hubContext = hubContext;
        }

        //[Authorize]
        [HttpPost("createMinesGame")]
        public async Task<CreateMinesGameResponce> CreateMinesGame(CreateMinesGameRequest request)
        {
            return await _minesService.CreateMinesGame(request);
        }

        //[Authorize]
        [HttpPost("openCell")]
        public async Task<OpenCellResponce> OpenCell(OpenCellRequest request)
        {
            var result = await _minesService.OpenCell(request);


            if (result.Item1.Result != null && result.Item1.Result.GameOver)
            {
                var user = _userService.GetById(result.Item2.UserId);

                var apiModel = new GameApiModel
                {
                    UserName = ReplaceAt(user.Name, 4, '*'),
                    Sum = result.Item2.BetSum,
                    CanWinSum = Math.Round(result.Item2.CanWin, 2),
                    Multiplier = (decimal)Math.Round(result.Item2.Chances[result.Item2.OpenedCellsCount - 1], 2),
                    Win = result.Item2.FinishGame,
                    GameType = Data.GameType.Mines,
                    GameDate = DateTime.Now.AddHours(3).ToString("G")
                };

                var gameJson = JsonConvert.SerializeObject(apiModel);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", gameJson);
            }

            return result.Item1;
        }

        [HttpPost("getMinesGame")]
        public async Task<MinesGameApiModel> GetMinesGame(GetByUserIdRequest request)
        {
            return await _minesService.GetActiveMinesGameByUserId(request);
        }

        [HttpPost("finishMinesGame")]
        public async Task<FinishMinesGameResponce> FinishMinesGame(GetByUserIdRequest request)
        {
            var result = await _minesService.FinishGame(request);

            if (result.Item1.Succes == false)
            {
                return result.Item1;
            }

            var user = _userService.GetById(request.Id);
            
            var apiModel = new GameApiModel
            {
                UserName = ReplaceAt(user.Name, 4, '*'),
                Sum = result.Item2.BetSum,
                CanWinSum = Math.Round(result.Item2.CanWin, 2),
                Multiplier =  (decimal)Math.Round(result.Item2.Chances[result.Item2.OpenedCellsCount], 2),
                Win = true,
                GameType = GameType.Mines
            };

            var gameJson = JsonConvert.SerializeObject(apiModel);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", gameJson);

            return result.Item1;
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