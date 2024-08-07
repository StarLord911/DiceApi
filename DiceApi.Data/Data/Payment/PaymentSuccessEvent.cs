using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class PaymentSuccessEvent
    {
        [JsonProperty("merchantId")]
        public long ShopId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("intid")]
        public long FreKassaOrderId { get; set; }

        [JsonProperty("merchantOrderId")]
        public long PaymentId { get; set; }
    }

    public class WithdrawalNotification
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public int CurrencyId { get; set; }
        public int PaymentSystemId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string Account { get; set; }
    }
}
