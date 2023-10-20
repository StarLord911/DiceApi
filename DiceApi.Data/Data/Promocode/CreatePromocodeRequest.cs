using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data

{
    public class CreatePromocodeRequest
    {
        [JsonProperty("promocode")]
        public string Promocode { get; set; }

        [JsonProperty("activationCount")]
        public int ActivationCount { get; set; }

        [JsonProperty("ballanceAdd")]
        public long BallanceAdd { get; set; }

        [JsonProperty("wagering")]
        public int Wagering { get; set; }
    }
}
