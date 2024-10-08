﻿using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    /// <summary>
    /// Интерфеис преднозначан для управления платежами в системе.
    /// </summary>
    public interface IPaymentService
    {
        Task<long> AddPayment(Payment payment);

        Task DeletePayment(long paymentId);


        Task ConfirmReferalOwnerPayment(ConfirmPayment payment);

        Task<List<Payment>> GetPaymentsByUserId(long userId);

        Task<Payment> GetPaymentsById(long paymentId);

        Task UpdatePaymentStatus(long paymentId, PaymentStatus status);

        Task<List<Payment>> GetAllPayedPayments();

        Task<PaymentStats> GetPaymentStats();

        Task<List<Payment>> GetAllUnConfiemedPayments();

        Task<PaginatedList<Payment>> GetPaginatedPayments(GetPaymentsRequest request);

        Task UpdateFkOrderId(long paymentId, long fkPaymentId);
    }
}
