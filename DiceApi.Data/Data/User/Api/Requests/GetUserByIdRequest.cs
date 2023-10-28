using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Requests
{
    public class GetUserByIdRequest
    {
        [JsonProperty("userId")]
        public long Id { get; set; }
    }
}
