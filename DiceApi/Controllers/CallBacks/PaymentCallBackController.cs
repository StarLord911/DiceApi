using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers.CallBacks
{
    [Route("api/paymentCallBack")]
    [EnableCors("AllowAll")]
    [ApiController]
    public class PaymentCallBackController : ControllerBase
    {
        private IPaymentService _paymentService;
        private IUserService _userService;
        private ILogRepository _logRepository;


        public PaymentCallBackController(IPaymentService paymentService,
            IUserService userService,
            ILogRepository logRepository)
        {
            _paymentService = paymentService;
            _userService = userService;
            _logRepository = logRepository;
        }
        
        [HttpPost("handle")]
        public async Task<bool> HandlePaymentEvent(PaymentSuccessEvent paymentSuccessEvent)
        {
            try
            {
                var payment = await _paymentService.GetPaymentsById(paymentSuccessEvent.PaymentId);

                await _logRepository.LogInfo($"CallBack payment start {paymentSuccessEvent.PaymentId}");
                if (payment.Status == PaymentStatus.Payed)
                {
                    return false;
                }

                await _paymentService.ConfirmReferalOwnerPayment(new ConfirmPayment { UserId = payment.UserId, Amount = payment.Amount });

                await _paymentService.UpdatePaymentStatus(payment.Id, PaymentStatus.Payed);

                await _userService.UpdateUserBallance(payment.UserId, payment.Amount);

                await _logRepository.LogInfo($"Successful balance update for the user {payment.UserId} amount {payment.Amount}");

                return await Task.FromResult<bool>(true);

            }
            catch (Exception ex)
            {
                await _logRepository.LogError("HandlePaymentEvent error");
                await _logRepository.LogException(ex);
                return await Task.FromResult<bool>(false);

            }
        }
    }
}
