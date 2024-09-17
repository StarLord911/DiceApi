using Newtonsoft.Json;
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
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("betSum")]
        public decimal BetSum { get; set; }

        [JsonProperty("multiplayer")]
        public float Multiplayer { get; set; }

        [JsonProperty("isColorBet")]
        public bool IsColorBet { get; set; }

        [JsonProperty("betColor")]
        public string BetColor { get; set; }

        [JsonProperty("isRange")]
        public bool IsRange { get; set; }

        [JsonProperty("range")]
        public string Range { get; set; }

        [JsonProperty("betNumber")]
        public int? BetNumber { get; set; }

    }
}
