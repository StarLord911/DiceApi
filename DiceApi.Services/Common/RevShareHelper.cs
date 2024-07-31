using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Common
{
    public static class RevShareHelper
    {
        public static decimal GetRevShareIncome(decimal sum, int revSharePercent)
        {
            decimal revShareIncome = (sum/ 100) * revSharePercent;

            return revShareIncome;
        }
    }
}
