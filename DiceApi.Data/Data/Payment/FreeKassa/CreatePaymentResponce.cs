using Newtonsoft.Json;

namespace DiceApi.Data
{
    public class CreatePaymentResponce
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        [JsonProperty("orderHash")]
        public string OrderHash { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}