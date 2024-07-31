using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Api;
using DiceApi.Data.Data.Promocode;
using DiceApi.Data.Data.User.Api.Requests;
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
        private readonly IPromocodeService _promocodeService;
        private readonly IRefferalService _refferalService;

        public ReferalController(IUserService userService,
            IPaymentService paymentService,
            IRefferalService refferalService,
            IPromocodeService promocodeService)
        {
            _userService = userService;
            _paymentService = paymentService;
            _refferalService = refferalService;
            _promocodeService = promocodeService;
        }

        //[Authorize]
        [HttpPost("getRefferalStats")]
        public async Task<GetReferalStatsResponce> GetRefferalStats(GetRefferalStatsByUserIdRequest request)
        {
            return await _refferalService.GetReferalStats(request);
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
            var owner = _userService.GetById(request.UserId);

            var referals = await _userService.GetRefferalsByUserId(request.UserId);
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

        [Authorize]
        [HttpPost("getRefferalPromocodesByUserId")]
        public async Task<List<RefferalPromocode>> GetRefferalPromocodesByUserId(GetByUserIdRequest request)
        {
            return await _promocodeService.GetRefferalPromocodesByUserId(request.UserId);
        }

        private bool IsThisMonth(DateTime dateTime)
        {
            return dateTime.Month == DateTime.Now.Month && dateTime.Year == DateTime.Now.Year;
        }

        [HttpPost("SetDate")]
        public async Task SetDate()
        {
            var referals = await _userService.GetRefferalsByUserId(85);

            foreach (var referal in referals)
            {
                var random = new Random().Next(6, 30);

                for (int i = 0; i < random; i++)
                {
                    var payment = new Payment()
                    {
                        Amount = new Random().Next(110, 1000),
                        CreatedAt = DateTime.Now.AddDays(new Random().Next(-360, 1)),
                        OrderId = "",
                        Status = PaymentStatus.Payed,
                        UserId = referal.Id
                    };

                    await _paymentService.AddPayment(payment);
                }
            }
        } 
    }
}
