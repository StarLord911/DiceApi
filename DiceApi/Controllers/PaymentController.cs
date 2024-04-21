using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using DiceApi.Data.Requests;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using FreeKassa.NET;
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
        private readonly IWithdrawalsService _withdrawalsService;

        public PaymentController(IPaymentService paymentService,
            IPaymentAdapterService paymentAdapterService,
            IWithdrawalsService withdrawalsService)
        {
            _paymentService = paymentService;
            _paymentAdapterService = paymentAdapterService;
            _withdrawalsService = withdrawalsService;

           
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

            var request = FreeKassHelper.CreateOrderRequest(paymentId, createPaymentRequest.Amount, 6, "61.4.112.166");
            //FreeKassHelper.GetPayLink(payment.OrderId, createPaymentRequest.Amount);
            var paymentForm = await _paymentAdapterService.CreatePaymentForm(request);

            return paymentForm;
        }

        [Authorize]
        [HttpPost("createWithdrawal")]
        public async Task<CreateWithdrawalResponce> CreateWithdrawal(CreateWithdrawalRequest createWithdrawalRequest)
        {
            return await _withdrawalsService.CreateWithdrawalRequest(createWithdrawalRequest);
        }

        [Authorize(true)]
        [HttpPost("confirmWithdrawal")]
        public async Task ConfirmWithdrawal(ConfirmWithdrawalRequest request)
        {
            await _withdrawalsService.СonfirmWithdrawal(request.WithdrawalId);
        }

        [Authorize]
        [HttpPost("getPaymentsByUserId")]
        public async Task<List<Payment>> GetPaymentsByUserId(GetByUserIdRequest request)
        {
            return await _paymentService.GetPaymentsByUserId(request.Id);
        }
    }
}
