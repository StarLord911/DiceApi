using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Dice
{
    public class DiceRequest
    {
        public long UserId { get; set; }

        public double Persent { get; set; }

        public double Sum { get; set; }
    }
}
