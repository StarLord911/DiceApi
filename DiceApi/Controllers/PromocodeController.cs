using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi.Controllers
{
    [Route("api/promocode")]
    [ApiController]
    public class PromocodeController : ControllerBase
    {
        private readonly IPromocodeService _promocodeService;

        public PromocodeController(IPromocodeService promocodeService)
        {
            _promocodeService = promocodeService;
        }

        [Authorize]
        [HttpPost("activatePromocode")]
        public async Task<ActivatePromocodeResponce> ActivatePromocode(ActivatePromocodeRequest promoCode)
        {
            return await _promocodeService.ActivetePromocode(promoCode);
        }

    }
}
