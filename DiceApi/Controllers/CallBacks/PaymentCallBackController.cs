using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Controllers.CallBacks
{
    [Route("api/paymentCallBack")]
    [ApiController]
    public class PaymentCallBackController : ControllerBase
    {
        private IPaymentService _paymentService;
        private IUserService _userService;
        private ILogRepository _logRepository;
        private readonly IWithdrawalsService _withdrawalsService;
        private readonly IPaymentAdapterService _paymentAdapterService;

        public PaymentCallBackController(IPaymentService paymentService,
            IUserService userService,
            ILogRepository logRepository,
            IPaymentAdapterService paymentAdapterService,
            IWithdrawalsService withdrawalsService)
        {
            _paymentService = paymentService;
            _userService = userService;
            _logRepository = logRepository;
            _paymentAdapterService = paymentAdapterService;
            _withdrawalsService = withdrawalsService;
        }
        
        [HttpPost("handle")]
        public async Task<bool> HandlePaymentEvent(PaymentSuccessEvent paymentSuccessEvent)
        {

            return true;
            //try
            //{
            //    await _logRepository.LogInfo($"Handle payment event {paymentSuccessEvent.FreKassaOrderId}, amount{paymentSuccessEvent.Amount}, status {paymentSuccessEvent.PaymentId}");

            //    var fkPayment = await  _paymentAdapterService.GetOrderByFreeKassaId(paymentSuccessEvent.FreKassaOrderId);

            //    if (fkPayment.Currency.ToLower() == "usdt")
            //    {
            //        fkPayment.Amount = fkPayment.Amount * RatesHelper.GetRates();
            //    }

            //    if (fkPayment.Status != 1)
            //    {
            //        await _logRepository.LogInfo($"Error on handle request from free kassa {paymentSuccessEvent.FreKassaOrderId}, amount{paymentSuccessEvent.Amount}");
            //    }

            //    var payment = await _paymentService.GetPaymentsById(paymentSuccessEvent.PaymentId);

            //    if (payment == null)
            //    {
            //        await _logRepository.LogError("Cannot find payment by id " + paymentSuccessEvent.PaymentId);
            //        return false;
            //    }

            //    await _logRepository.LogInfo($"CallBack payment start {paymentSuccessEvent.PaymentId}");
            //    if (payment.Status == PaymentStatus.Payed)
            //    {
            //        return false;
            //    }

            //    await _paymentService.ConfirmReferalOwnerPayment(new ConfirmPayment { UserId = payment.UserId, Amount = payment.Amount });

            //    await _paymentService.UpdatePaymentStatus(payment.Id, PaymentStatus.Payed);

            //    var user = _userService.GetById(payment.UserId);

            //    await _userService.UpdateUserBallance(payment.UserId, user.Ballance + fkPayment.Amount);

            //    await _logRepository.LogInfo($"Successful balance update for the user {payment.UserId} amount {payment.Amount}");

            //    return await Task.FromResult<bool>(true);

            //}
            //catch (Exception ex)
            //{
            //    await _logRepository.LogException("HandlePaymentEvent error", ex);
            //    return await Task.FromResult<bool>(false);

            //}
        }

        [HttpPost("handleWithdrawal")]
        public async Task HandleWithdrawal([FromForm] WithdrawalNotification model)
        {
            try
            {
                var result = GetPropertiesAsString(model);

                var withrowalId = await _withdrawalsService.GetWithdrawalIdByFkWaletId(Convert.ToInt32(model.Id));

                var withrowal = await _withdrawalsService.GetById(withrowalId);


                var status = Convert.ToInt32(model.Status);

                var log = new StringBuilder();
                log.Append($"Processing withrowal start {withrowalId}, {status}, {model.Amount}");

                if (status == 1)
                {
                    log.Append($"Processing withrowal end {withrowalId}, {status}, {model.Amount}");
                    await _withdrawalsService.UpdateStatusWithFkValetId(withrowalId, WithdrawalStatus.Processed);
                }
                else if(status == 8 || status == 9 || status == 10)
                {
                    if (withrowal.TryCount < 3)
                    {
                        await _withdrawalsService.UpdateTryCount(withrowal.Id, withrowal.TryCount++);
                        await _withdrawalsService.СonfirmWithdrawal(withrowal.Id);

                        log.Append($"new try {withrowal.TryCount++}");
                        await _logRepository.LogInfo(log.ToString());
                    }

                    await _withdrawalsService.UpdateStatusWithFkValetId(withrowalId, WithdrawalStatus.Error);

                    await _withdrawalsService.DeactivateWithdrawal(withrowalId);
                    log.Append($"Processing withrowal end {withrowalId}, {status}, {model.Amount}");
                    await _logRepository.LogInfo(log.ToString());
                }
            }
            catch (Exception ex)
            {
                await _logRepository.LogException($"Error when processing withrowal {model.OrderId} amount: { model.Amount}", ex);
            }
        }


        private static string GetPropertiesAsString(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            string result = "";

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                result += $"{property.Name}: {value}\n";
            }

            return result;
        }
    }
}
