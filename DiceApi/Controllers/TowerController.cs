using DiceApi.Attributes;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.Tower;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DiceApi.Controllers
{
    [Route("api/towerController")]
    [ApiController]
    public class TowerController : ControllerBase
    {
        private readonly ITowerGameService _towerGameService;

        public TowerController(ITowerGameService towerGameService)
        {
            _towerGameService = towerGameService;
        }

        [Authorize]
        [HttpPost("createTowerGame")]
        public async Task<CreateTowerGameResponce> CreateMinesGame(CreateTowerGameRequest request)
        {
            return await _towerGameService.Create(request);
        }
    }
}
