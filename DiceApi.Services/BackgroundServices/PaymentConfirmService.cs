using DiceApi.Common;
using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using DiceApi.Services.Implements;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceApi.Services.BackgroundServices
{
    public class PaymentConfirmService : BackgroundService
    {
        private IPaymentService _paymentService;
        private IUserService _userService;
        private ILogRepository _logRepository;
        private readonly IWithdrawalsService _withdrawalsService;
        private readonly IPaymentAdapterService _paymentAdapterService;

        public PaymentConfirmService(IPaymentService paymentService,
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                var payments = await _paymentService.GetAllUnConfiemedPayments();

                foreach (var payment in payments)
                {
                    try
                    {
                        var fkPayment = await _paymentAdapterService.GetOrderByFreeKassaId(payment.FkPaymentId.Value);

                        if (fkPayment.Status == 1)
                        {
                            await _paymentService.ConfirmReferalOwnerPayment(new ConfirmPayment { UserId = payment.UserId, Amount = payment.Amount });

                            await _paymentService.UpdatePaymentStatus(payment.Id, PaymentStatus.Payed);

                            var user = _userService.GetById(payment.UserId);

                            if (fkPayment.Currency.ToLower() == "usdt")
                            {
                                fkPayment.Amount = fkPayment.Amount * RatesHelper.GetRates();
                            }

                            await _userService.UpdateUserBallance(payment.UserId, user.Ballance + fkPayment.Amount);
                            await _logRepository.LogInfo($"Successful confirm payment {payment.Id} amount {payment.Amount}");
                            continue;
                        }
                        else if (fkPayment.Status == 8 || fkPayment.Status == 9)
                        {
                            await _paymentService.UpdatePaymentStatus(payment.Id, Data.PaymentStatus.Error);
                            continue;
                        }


                        if ((DateTime.UtcNow.GetMSKDateTime() - payment.CreatedAt).TotalMinutes > 15 && fkPayment.Currency.ToLower() != "usdt")
                        {
                            await _paymentService.UpdatePaymentStatus(payment.Id, Data.PaymentStatus.Error);
                            continue;
                        }

                        if ((DateTime.UtcNow.GetMSKDateTime() - payment.CreatedAt).TotalMinutes > 30 && fkPayment.Currency.ToLower() != "usdt")
                        {
                            await _paymentService.UpdatePaymentStatus(payment.Id, Data.PaymentStatus.Error);
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                Thread.Sleep(15000);
            }
        }
    }
}
