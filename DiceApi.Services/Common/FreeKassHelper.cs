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
            return new CreateOrderRequest
            {
                Amount = amount,
                Currency = "RUB",
                Email = "example@gmail.com",
                I = paySystemId,
                Ip = clientIp,
                Nonce = paymentId,
                PaymentId = paymentId.ToString(),
                ShopId = int.Parse(ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaShopId)),
                Signature = GetSignature(paymentId, amount)
            };
        }

        private static string GetSignature(int paymentId, decimal amount)
        {
            string signature = string.Empty;

            // Convert OrderRequestModel to key-value pairs
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "paymentId", paymentId.ToString() },
                { "amount", amount.ToString() },
            };

            // Sort the parameters by key
            SortedDictionary<string, string> sortedParameters = new SortedDictionary<string, string>(parameters);

            // Concatenate the parameter key-value pairs
            string concatenatedParameters = string.Join("", sortedParameters.Values);

            // Concatenate the secret key with the concatenated parameters
            string input = ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaSecretOne) + concatenatedParameters;

            // Compute the SHA256 hash of the input string
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            return signature;
        }
    }
}
