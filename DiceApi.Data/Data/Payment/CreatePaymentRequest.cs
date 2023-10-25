using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Payment
{
    public class CreatePaymentRequest
    {
        public decimal Amount { get; set; }

        public long PaymentId { get; set; }
    }
}
