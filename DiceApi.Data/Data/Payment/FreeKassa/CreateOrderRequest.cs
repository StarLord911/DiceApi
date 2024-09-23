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
        
        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }
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
        public Dictionary<object, object> Fields { get; set; } = new Dictionary<object, object>();

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

    public class FkWaletSpbWithdrawalBank
    {
        [JsonProperty("id")]
        public string BankId { get; set; }

        [JsonProperty("name")]
        public string BankName { get; set; }
    }

    public class FkWaletSpbWithdrawalBankResponce
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public List<FkWaletSpbWithdrawalBank> GetSpbWithdrawalBanks { get; set; }
    }

    public class InnerData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }

    public class FkWaletWithdrawalStatusResponce
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public InnerData Data { get; set; }
    }


    //Ballance
    public class FkWaletBallanceApiResponse
    {
        public string Status { get; set; }
        public List<CurrencyData> Data { get; set; }
    }

    public class CurrencyData
    {
        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }
        public decimal Value { get; set; }
    }
}
