using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Requests
{
    public class GetByUserIdRequest
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }
    }
}
