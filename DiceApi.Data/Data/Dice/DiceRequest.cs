using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Dice
{
    /// <summary>
    /// Запрос игры в дайс
    /// </summary>
    public class DiceRequest
    {
        public long UserId { get; set; }

        public double Persent { get; set; }

        public decimal Sum { get; set; }
    }
}
