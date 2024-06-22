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
            // Отнимаем 20% от суммы
            decimal sumAfter20Percent = sum - (sum * 0.2m);

            // Рассчитываем доход с учетом процента revSharePercent
            decimal revShareIncome = sumAfter20Percent * revSharePercent / 100;

            return revShareIncome;
        }
    }
}
