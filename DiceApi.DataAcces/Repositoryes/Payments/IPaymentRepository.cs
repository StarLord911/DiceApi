﻿using DiceApi.Data;
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

        Task<List<Payment>> GetPaymentsByUserId(long userId);

        Task UpdatePaymentStatus(string paymentId);
    }
}
