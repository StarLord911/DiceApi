using DiceApi.Data.Data.Payment.FreeKassa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

    public class CreatePaymentResponse
    {
        public bool Succesful { get; set; }

        public string Message { get; set; }

        public string Location { get; set; }

        [JsonIgnore()]
        public int OrderId { get; set; }

    }

    public class PaymentDos
    {
        public PaymentDos()
        {
            DateTimes = new List<DateTime>();
        }

        public List<DateTime> DateTimes { get; set; }

        public long UserId { get; set; }

        public long Count { get; set; }
    }
}
