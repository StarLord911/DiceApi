using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    public class CreateRouletteBetRequest
    {
        public long UserId { get; set; }

        public List<RouletteBet> Bets { get; set; } 
    }

    public class RouletteBet
    {
        public int BetNumber { get; set; }

        public decimal BetSum { get; set; }

        public BetColor? BetColor { get; set; }
    }

    public enum BetColor
    {
        Green = 1,
        Red = 2,
        Black = 3
    }
}
