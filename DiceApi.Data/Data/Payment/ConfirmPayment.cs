using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Модель подтверждения платежа.
    /// </summary>
    public class ConfirmPayment
    {
        public long UserId { get; set; }

        public decimal Amount { get; set; }
    }
}
