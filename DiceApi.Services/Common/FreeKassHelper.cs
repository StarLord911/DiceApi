using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DiceApi.Services
{
    public static class FreeKassHelper
    {
        public static string CreateOrderRequest(CreatePaymentRequest createPayment, long paymentId)
        {
            var request = new CreateOrderRequest
            {
                Amount = createPayment.Amount,
                Currency = "RUB",
                Email = "example@gmail.com",
                I = (int)createPayment.PaymentType,
                Ip = "61.4.112.166",
                Nonce = DateTime.Now.Ticks,
                ShopId = int.Parse(ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaShopId)),
                PaymentId = paymentId.ToString()
            };

            if ((int)createPayment.PaymentType == 14 || (int)createPayment.PaymentType == 15)
            {
                request.Currency = "USDT";
            }

            var parameters = new Dictionary<string, string>()
            {
                { "currency", request.Currency.ToString() },
                { "email", request.Email.ToString() },
                { "i", request.I.ToString() },
                { "ip", request.Ip.ToString() },
                { "nonce", request.Nonce.ToString() },
                { "shopId", request.ShopId.ToString() },
                { "amount", request.Amount.ToString() },
                { "paymentId", request.PaymentId.ToString() }
            };

            request.Signature = GetSignature(parameters);

            return JsonConvert.SerializeObject(request);
            
        }


        public static string GetOrderByIdRequest(long orderId)
        {
            var request = new GetOrderByFreeKassaIdRequest()
            {
                Nonce = DateTime.Now.Ticks,
                ShopId = int.Parse(ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaShopId)),
                OrderId = orderId
            };

            var parameters = new Dictionary<string, string>()
            {
                { "shopId", request.ShopId.ToString() },
                { "nonce", request.Nonce.ToString() },
                { "orderId", orderId.ToString() },
            };

            request.Signature = GetSignature(parameters);

            return JsonConvert.SerializeObject(request);

        }

        public static WithdrawalRequestFkWallet GetWithdrawalRequest(Withdrawal withdrawal)
        {
            var request = new WithdrawalRequestFkWallet()
            {
                Amount = withdrawal.Amount,
                Currency_id = 1,
                Account = withdrawal.CardNumber.Replace("+", ""),
                PaymentSystemId = 6,
                Description = "Description",
                FeeFromBalance = 1,
                IdempotenceKey = Guid.NewGuid().ToString(),
                OrderId = withdrawal.Id,
            };

            if (!string.IsNullOrEmpty(withdrawal.BankId))
            {
                request.Fields.Add("sbp_bank_id", withdrawal.BankId);
                request.PaymentSystemId = 5;
            }

            return request;
        }

        public static string GetWithdrawalAvailableCurrencyes()
        {
            var request = new GetWithdrawalAvailableCurrencyesRequest()
            {
                Nonce = DateTime.Now.Ticks,
                ShopId = int.Parse(ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaShopId)),
            };

            var parameters = new Dictionary<string, string>()
            {
                { "shopId", request.ShopId.ToString() },
                { "nonce", request.Nonce.ToString() },
            };

            request.Signature = GetSignature(parameters);

            return JsonConvert.SerializeObject(request);
        }

        private static string GetBankSbpId(int id)
        {
            if (id == 1)
            {
                return "1enc00000004";
            }

            return "1enc00000111";
        }

        private static string GetSignature(Dictionary<string, string> pairs)
        {
            return GenerateSignature(pairs, "683eb747358f586f5ff0f71b9640aa75");
        }

        public static string GenerateSignature(Dictionary<string, string> parameters, string apiKey)
        {
            // Сортировка параметров по ключам в алфавитном порядке

            var sortedData = string.Join('|', parameters.OrderBy(x => x.Key).Select(x => x.Value));

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sortedData));
                string sign = BitConverter.ToString(hash).Replace("-", "").ToLower();

               return sign;
            }
        }
    }
}