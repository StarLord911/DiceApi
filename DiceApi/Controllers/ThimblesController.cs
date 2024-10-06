using DiceApi.Data.ApiReqRes;
using DiceApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DiceApi.Attributes;
using DiceApi.Data.ApiReqRes.Thimble;
using DiceApi.Services.Implements.Thimbles;

namespace DiceApi.Controllers
{
    [Route("api/thimbles")]
    [ApiController]
    public class ThimblesController : ControllerBase
    {
        private readonly IThimblesService _thimblesService;



        public ThimblesController(IThimblesService thimblesService)
        {
            _thimblesService = thimblesService;
        }

        [Authorize]
        [HttpPost("bet")]
        public async Task<BetThimblesResponce> Bet(BetThimblesRequest request)
        {
            return await _thimblesService.Bet(request);
        }

      
    }
}
