using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Payment
{
    public class WithdrawalStats
    {
        public decimal ToDay { get; set; }

        public decimal ToWeek { get; set; }

        public decimal ToMonth { get; set; }

        public decimal AllDays { get; set; }

        public decimal WithdrawalWaitSum { get; set; }

    }
}
