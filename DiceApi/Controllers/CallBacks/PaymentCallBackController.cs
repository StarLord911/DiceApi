using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers.CallBacks
{
    [Route("api/paymentCallBack")]
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
        public async Task HandlePaymentEvent(PaymentSuccessEvent paymentSuccessEvent)
        {
            var payment = await _paymentService.GetPaymentsById(paymentSuccessEvent.PaymentId);

            if (payment.Status == PaymentStatus.Payed)
            {
                return;
            }

            await _paymentService.UpdatePaymentStatus(payment.Id, PaymentStatus.Payed);

            await _userService.UpdateUserBallance(payment.UserId, payment.Amount);

            await _logRepository.LogInfo($"Successful balance update for the user {payment.UserId} amount {payment.Amount}");

            await Task.CompletedTask;
        }
    }
}
