using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Requests;
using DiceApi.Services;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DiceApi.Controllers
{
    [Route("api/mines")]
    [ApiController]
    public class MinesController : ControllerBase
    {
        private readonly IMinesService _minesService;

        public MinesController(IMinesService minesService)
        {
            _minesService = minesService;
        }

        [Authorize]
        [HttpPost("createMinesGame")]
        public async Task<CreateMinesGameResponce> CreateMinesGame(CreateMinesGameRequest request)
        {
            return await _minesService.CreateMinesGame(request);
        }

        [Authorize]
        [HttpPost("openCell")]
        public async Task<OpenCellResponce> OpenCell(OpenCellRequest request)
        {
            return await _minesService.OpenCell(request);
        }

        [Authorize]
        [HttpPost("getMinesGame")]
        public async Task<MinesGameApiModel> GetMinesGame(GetByUserIdRequest request)
        {
            return await _minesService.GetActiveMinesGameByUserId(request);
        }

        [Authorize]
        [HttpPost("finishMinesGame")]
        public async Task<FinishMinesGameResponce> FinishMinesGame(GetByUserIdRequest request)
        {
            return await _minesService.FinishGame(request);
        }
    }
}