using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IPaymentRepository
    {
        Task<long> CreatePayment(Payment payment);

        Task DeletePayment(long paymentId);

        Task<List<Payment>> GetPaymentsByUserId(long userId);

        Task UpdatePaymentStatus(long paymentId, PaymentStatus status);

        Task<Payment> GetPaymentsById(long paymentId);

        Task<List<Payment>> GetAllPayedPayments();

        Task<List<Payment>> GetAllUnConfiemedPayments();

        Task UpdateFkOrderId(long paymentId, long fkPaymentId);

        Task<PaginatedList<Payment>> GetPaginatedPayments(GetPaymentsRequest request);
    }
}
