using DiceApi.Attributes;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.Tower;
using DiceApi.Data.Requests;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

        [Authorize]
        [HttpPost("openCell")]
        public async Task<OpenTowerCellResponce> OpenCell(OpenTowerCellRequest request)
        {
            return await _towerGameService.OpenCell(request);
        }

        [Authorize]
        [HttpPost("finishGame")]
        public async Task<FinishTowerGameResponce> FinishGame(GetByUserIdRequest request)
        {
            return await _towerGameService.FinishGame(request);
        }

        [HttpPost("getCoefficients")]
        public Dictionary<int, Dictionary<int, double>> GetCoefficients() 
        {
            return new Dictionary<int, Dictionary<int, double>>
            {
                { 1, new Dictionary<int, double>
                    {
                        { 1, 1.2 },
                        { 2, 1.5 },
                        { 3, 1.87 },
                        { 4, 2.34 },
                        { 5, 2.92 },
                        { 6, 3.66 },
                        { 7, 4.57 },
                        { 8, 5.72 },
                        { 9, 7.15 },
                        { 10, 8.94 }
                    }
                },
                { 2, new Dictionary<int, double>
                    {
                        { 1, 1.6 },
                        { 2, 2.66 },
                        { 3, 4.44 },
                        { 4, 7.4 },
                        { 5, 12.34 },
                        { 6, 20.57 },
                        { 7, 34.29 },
                        { 8, 57.15 },
                        { 9, 95.25 },
                        { 10, 158.76 }
                    }
                },
                { 3, new Dictionary<int, double>
                    {
                        { 1, 2.4 },
                        { 2, 6 },
                        { 3, 15 },
                        { 4, 37.5 },
                        { 5, 93.75 },
                        { 6, 234.37 },
                        { 7, 585.93 },
                        { 8, 1464.84 },
                        { 9, 3662.1 },
                        { 10, 9155.27 }
                    }
                },
                { 4, new Dictionary<int, double>
                    {
                        { 1, 4.8 },
                        { 2, 24 },
                        { 3, 120 },
                        { 4, 600 },
                        { 5, 3000 },
                        { 6, 15000 },
                        { 7, 75000 },
                        { 8, 375000 },
                        { 9, 1875000 },
                        { 10, 9375000 }
                    }
                }
            };
        }
    }
}
