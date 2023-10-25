using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task AddPayment(Payment payment)
        {
            await _paymentRepository.CreatePayment(payment);
        }
    }
}
