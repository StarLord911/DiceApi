using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DiceApi.Data
{
    public class CreateFreeKassaPaymentResponce
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

    public class OrderResponse
    {
        public string Type { get; set; }
        public int Pages { get; set; }
        public List<GetFreeKassaOrderByIdResponce> Orders { get; set; }
    }

    public class GetFreeKassaOrderByIdResponce
    {
        [JsonProperty("merchant_order_id")]
        public string MerchantOrderId { get; set; }

        [JsonProperty("fk_order_id")]
        public long FkOrderId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }

    public class CreateFreKassaWithdrawalResponce
    {

    }
}