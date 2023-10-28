using DiceApi.Data;
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
        Task AddPayment(Payment payment);

        Task ConfirmPayment(ConfirmPayment payment);

    }
}
