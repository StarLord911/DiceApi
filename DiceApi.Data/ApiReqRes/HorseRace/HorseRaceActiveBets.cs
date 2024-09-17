using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes.HorseRace
{
    public class HorseRaceActiveBets
    {
        public HorseRaceActiveBets()
        {
            Bets = new List<HorseRaceActiveBet>();
        }

        public int BetsCount => Bets.Count;

        public List<HorseRaceActiveBet> Bets { get; set; }
    }

    /// <summary>
    /// Активная ставка котороя пока не сыграла.
    /// </summary>
    public class HorseRaceActiveBet
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("betSum")]
        public decimal BetSum { get; set; }

        [JsonProperty("multiplayer")]
        public float Multiplayer { get; set; }

        [JsonProperty("horseColor")]
        public HorseColor HorseColor { get; set; }
    }
}
