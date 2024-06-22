using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    }

    public class GetOrderByFreeKassaIdRequest
    {
        [JsonProperty("shopId")]
        public int ShopId { get; set; }

        [JsonProperty("nonce")]
        public long Nonce { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }

    }

    public class WithdrawalRequestFkWallet
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency_id")]
        public int Currency_id { get; set; }

        [JsonProperty("payment_system_id")]
        public int PaymentSystemId { get; set; }

        [JsonProperty("fee_from_balance")]
        public int FeeFromBalance { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("order_id")]
        public int OrderId { get; set; }

        [JsonProperty("fields")]
        public List<object> Fields { get; set; } = new List<object>();

        [JsonProperty("idempotence_key")]
        public string IdempotenceKey { get; set; }
    }

    public class GetWithdrawalAvailableCurrencyesRequest
    {

        [JsonProperty("shopId")]
        public int ShopId { get; set; }

        [JsonProperty("nonce")]
        public long Nonce { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
