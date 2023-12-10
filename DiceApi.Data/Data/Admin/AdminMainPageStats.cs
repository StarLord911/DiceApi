using DiceApi.Data.Data.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Admin
{
    /// <summary>
    /// Статистика для админки.
    /// </summary>
    public class AdminMainPageStats
    {
        public PaymentStats PaymentStats { get; set; }

        public WithdrawalStats WithdrawalStats { get; set; }

        public decimal WithdrawalWaitSum { get; set; }

        public decimal FreeKassaBallance { get; set; }

        public int UsersCount { get; set; }

    }
}
