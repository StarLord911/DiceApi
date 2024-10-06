using DiceApi.Data.ApiReqRes;
using DiceApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DiceApi.Attributes;
using DiceApi.Data.ApiReqRes.Thimble;

namespace DiceApi.Controllers
{
    [Route("api/thimbles")]
    [ApiController]
    public class ThimblesController : ControllerBase
    {

        public ThimblesController()
        {
            
        }

        //[Authorize]
        //[HttpPost("bet")]
        //public async Task<BetThimblesResponce> Bet(BetThimblesRequest request)
        //{
            
        //}
    }
}
