using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
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

        [JsonProperty("isRefferalPromocode")]
        public bool IsRefferalPromocode { get; set; }

        [JsonProperty("refferalPromocodeOwnerId")]
        public long? RefferalPromocodeOwnerId { get; set; }
    }
}
