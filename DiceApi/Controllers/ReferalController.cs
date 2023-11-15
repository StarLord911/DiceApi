using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Api;
using DiceApi.Data.Requests;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers
{
    [Route("api/Referal")]
    [ApiController]
    public class ReferalController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;

        public ReferalController(IUserService userService,
            IPaymentService paymentService)
        {
            _userService = userService;
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("getReferalStats")]
        public async Task<GetReferalStatsResponce> GetRegeralStats(GetByUserIdRequest request)
        {
            var referals = await _userService.GetRefferalsByUserId(request.Id);
            var responce = new GetReferalStatsResponce();
            responce.ToDayReferals = referals.Count(r => r.RegistrationDate.Date == DateTime.Today);
            responce.ToMonthReferals = referals.Count(r => IsThisMonth(r.RegistrationDate.Date));

            responce.ToAllTimeReferals = referals.Count();

            return responce;

        }

        [Authorize]
        [HttpPost("getRefferalsByUserId")]
        public async Task<GetPaginatedDataByUserIdResponce<UserReferral>> GetRefferalsByUserId(GetReferalsByUserIdRequest request)
        {
            return await _userService.GetRefferalsByUserId(request);
        }

        [Authorize]
        [HttpPost("getProfitByUserId")]
        public async Task<GetProfitStatsResponce> GetProfitByUserId(GetByUserIdRequest request)
        {
            var owner = _userService.GetById(request.Id);

            var referals = await _userService.GetRefferalsByUserId(request.Id);
            var result = new GetProfitStatsResponce();

            foreach (var referal in referals)
            {
                var payments = (await _paymentService.GetPaymentsByUserId(referal.Id)).Where(p => p.Status == PaymentStatus.Payed);

                var toDayPayment = payments.Where(r => r.CreatedAt.Date == DateTime.Today);
                result.ToDayReferals += toDayPayment.Select(p => (p.Amount / 100) * owner.ReferalPercent).Sum();

                var toMonthReferals = payments.Where(r => IsThisMonth(r.CreatedAt));
                result.ToMonthReferals += toMonthReferals.Select(p => (p.Amount / 100) * owner.ReferalPercent).Sum();

                result.ToAllTimeReferals += payments.Select(p => (p.Amount / 100) * owner.ReferalPercent).Sum();
            }

            return result;
        }

        private bool IsThisMonth(DateTime dateTime)
        {
            return dateTime.Month == DateTime.Now.Month && dateTime.Year == DateTime.Now.Year;
        }

    }
}
