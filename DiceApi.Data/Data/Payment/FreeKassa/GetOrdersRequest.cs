using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Payment.FreeKassa
{
    public class GetOrdersRequest
    {
        [JsonProperty("shopId")]
        public int ShopId { get; set; }


    }
}
