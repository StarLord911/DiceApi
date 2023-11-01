using DiceApi.Common;
using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public static class FreeKassHelper
    {
        public static CreateOrderRequest CreateOrderRequest(int paymentId, decimal amount, int paySystemId, string clientIp)
        {
            var req = new CreateOrderRequest
            {
                Amount = amount,
                Currency = "RUB",
                Email = "example@gmail.com",
                I = paySystemId,
                Ip = clientIp,
                Nonce = (int)DateTime.Now.Ticks,
                PaymentId = paymentId.ToString(),
                ShopId = int.Parse(ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaShopId)),
            };

            req.Signature = GetSignature(req);

            return req;
        }

        private static string GetSignature(CreateOrderRequest request)
        {
            string signature = string.Empty;

            // Convert OrderRequestModel to key-value pairs
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "paymentId", request.PaymentId.ToString() },
                { "currency", request.Currency.ToString() },
                { "Email", request.Email.ToString() },
                { "i", request.I.ToString() },
                { "ip", request.Ip.ToString() },
                { "nonce", request.Nonce.ToString() },
                { "shopId", request.ShopId.ToString() },

            };
            //cb67a126d4bc65972b2ed97d42c9a045

            var sortedData = new SortedDictionary<string, string>(parameters);

            var signString = string.Join("|", sortedData.Values);
            var apiKey = ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaSecretOne);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("cb67a126d4bc65972b2ed97d42c9a045")))
            {
                var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));
                signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

                sortedData.Add("signature", signature);
            }
            return signature;
        }
    }
}
