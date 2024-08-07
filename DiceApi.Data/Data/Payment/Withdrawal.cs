using DiceApi.Data.Data.Payment.FreeKassa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Заявка на вывод
    /// </summary>
    public class Withdrawal
    {
        public int Id { get; set; }

        public long UserId { get; set; }

        public decimal Amount { get; set; }

        public string CardNumber { get; set; }

        public DateTime CreateDate { get; set; }

        public WithdrawalStatus Status { get; set; }

        public string? BankId { get; set; }

        public long? FkWaletId { get; set; }

    }
}
