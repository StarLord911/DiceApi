using DiceApi.Data.Data.Payment.FreeKassa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Payment
{
    /// <summary>
    /// Запрос на пополнение баланса.
    /// </summary>
    public class CreatePaymentRequest
    {
        public decimal Amount { get; set; }

        public long UserId { get; set; }

        public PaymentType PaymentType { get; set; }
    }
}
