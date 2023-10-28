using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentAdapterService _paymentAdapterService;

        public PaymentController(IPaymentService paymentService,
            IPaymentAdapterService paymentAdapterService)
        {
            _paymentService = paymentService;
            _paymentAdapterService = paymentAdapterService;
        }

        [Authorize]
        [HttpPost("createPayment")]
        public async Task<string> CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            var payment = new Payment
            {
                Amount = createPaymentRequest.Amount,
                OrderId = Guid.NewGuid().ToString(),
                Status = PaymentStatus.New,
                CreatedAt = DateTime.Now,
                UserId = createPaymentRequest.UserId
            };

            var paymentId = await _paymentService.AddPayment(payment);
            var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

            var request = FreeKassHelper.CreateOrderRequest((int)paymentId, createPaymentRequest.Amount, 1, ipAddress);

            var paymentForm = await _paymentAdapterService.CreatePaymentForm(request);

            return paymentForm;
        }
    }
}
