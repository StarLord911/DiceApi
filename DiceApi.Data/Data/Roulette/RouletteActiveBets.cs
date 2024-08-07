using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Roulette
{
    /// <summary>
    /// Информация об активных ставках в рулетке.
    /// </summary>
    public class RouletteActiveBets
    {
        public RouletteActiveBets()
        {
            Bets = new List<RouletteActiveBet>();
        }

        public int BetsCount => Bets.Count;

        public List<RouletteActiveBet> Bets { get; set; }
    }

    /// <summary>
    /// Активная ставка котороя пока не сыграла.
    /// </summary>
    public class RouletteActiveBet
    {
        public string UserName { get; set; }

        public decimal BetSum { get; set; }

        public float Multiplayer { get; set; }

        public bool IsColorBet { get; set; }

        public string BetColor { get; set; }

        public int? BetNumber { get; set; }

    }
}
