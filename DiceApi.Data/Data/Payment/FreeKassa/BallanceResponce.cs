using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class BalanceResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("ballance")]
        public object Ballance { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
