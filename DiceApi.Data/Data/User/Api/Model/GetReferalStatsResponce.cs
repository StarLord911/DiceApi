using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class GetReferalStatsResponce
    {
        public int ToDayReferals { get; set; }

        public int ToMonthReferals { get; set; }

        public int ToAllTimeReferals { get; set; }

    }

    public class GetProfitStatsResponce
    {
        public decimal ToDayReferals { get; set; }

        public decimal ToMonthReferals { get; set; }

        public decimal ToAllTimeReferals { get; set; }

    }
}
