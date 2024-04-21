using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class CreateOrderRequest
    {
        [JsonProperty("shopId")]
        public int ShopId { get; set; }

        [JsonProperty("nonce")]
        public long Nonce { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("i")]
        public int I { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }



        //[JsonProperty("success_url")]
        //public string SuccessUrl { get; set; }

        //[JsonProperty("failure_url")]
        //public string FailureUrl { get; set; }

        //[JsonProperty("notification_url")]
        //public string NotificationUrl { get; set; }
    }
}
