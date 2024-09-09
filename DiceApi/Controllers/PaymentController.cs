using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using DiceApi.Data.Data.Payment.Api;
using DiceApi.Data.Requests;
using DiceApi.Services;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using FreeKassa.NET;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public async Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            var payment = new Payment
            {
                Amount = createPaymentRequest.Amount,
                OrderId = Guid.NewGuid().ToString(),
                Status = PaymentStatus.New,
                CreatedAt = DateTime.UtcNow.AddHours(3),
                UserId = createPaymentRequest.UserId
            };

            var paymentId = await _paymentService.AddPayment(payment);
            
            var result = await _paymentAdapterService.CreatePaymentForm(createPaymentRequest, paymentId);

            if (!result.Succesful)
            {
                var paymentMethods = FreeKassaActivePaymentMethodsHelper.GetPaymentMethodsInfo();
                var selectedMethod = paymentMethods.FirstOrDefault(m => m.MethodId == (int)createPaymentRequest.PaymentType);

                if (selectedMethod == null)
                {
                    return new CreatePaymentResponse
                    {
                        Succesful = false,
                        Message = $"Не найдет тип оплаты {(int)createPaymentRequest.PaymentType}",
                        Location = null
                    };
                }

                if (createPaymentRequest.Amount < selectedMethod.MinDeposited
                    || createPaymentRequest.Amount > selectedMethod.MaxDeposit)
                {
                    return new CreatePaymentResponse
                    {
                        Succesful = false,
                        Message = $"Минимальная сумма пополнения {selectedMethod.MinDeposited} рублей, максимальная {selectedMethod.MaxDeposit}",
                        Location = null
                    };

                }

                await _paymentService.DeletePayment(paymentId);
            }

            return result;
        }

        [Authorize]
        [HttpPost("getCryptoRates")]
        public List<CryptoRate> GetCryptoRates()
        {

            return new List<CryptoRate>()
            {
                new CryptoRate()
                {
                    Crypto = "usdt",
                    RateToRub = RatesHelper.GetRates()
                } 
            };
        }

        [Authorize]
        [HttpPost("getActivePaymentMethods")]
        public List<PaymentFreeKassaMethodInformation> GetActivePaymentMethods()
        {
            return FreeKassaActivePaymentMethodsHelper.GetPaymentMethodsInfo();
        }

        //[Authorize]
        [HttpPost("createWithdrawal")]
        public async Task<CreateWithdrawalResponce> CreateWithdrawal(CreateWithdrawalRequest createWithdrawalRequest)
        {
            return await _withdrawalsService.CreateWithdrawalRequest(createWithdrawalRequest);
        }

        [Authorize]
        [HttpPost("getPaymentsByUserId")]
        public async Task<List<Payment>> GetPaymentsByUserId(GetByUserIdRequest request)
        {
            var result = await _paymentService.GetPaymentsByUserId(request.UserId);
            result.Reverse();
            return result;
        }

        [Authorize]
        [HttpPost("getWithdrawasByUserId")]
        public async Task<List<Withdrawal>> GetWithdrawasByUserId(GetByUserIdRequest request)
        {
            var result = await _withdrawalsService.GetAllByUserId(request.UserId);
            result.Reverse();
            return result;
        }

        [HttpPost("getSpbWithdrawalBanks")]
        public async Task<List<FkWaletSpbWithdrawalBank>> GetSpbWithdrawalBanks()
        {
            return await _paymentAdapterService.GetSpbWithdrawalBanks();
        }
    }
}
