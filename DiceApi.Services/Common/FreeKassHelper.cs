using DiceApi.Common;
using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DiceApi.Services
{
    public static class FreeKassHelper
    {
        public static CreateOrderRequest CreateOrderRequest(long paymentId, decimal amount, int paySystemId, string clientIp)
        {
            var req = new CreateOrderRequest
            {
                Amount = amount,
                Currency = "RUB",
                Email = "example@gmail.com",
                I = paySystemId,
                Ip = clientIp,
                Nonce = DateTime.Now.Ticks,
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
                { "order_id", request.PaymentId.ToString() },
                { "currency", request.Currency.ToString() },
                { "email", request.Email.ToString() },
                { "i", request.I.ToString() },
                { "ip", request.Ip.ToString() },
                { "nonce", request.Nonce.ToString() },
                { "shopId", request.ShopId.ToString() },
            };

            return GenerateSignature(parameters, "376483a1585e848d0c0cf699c2df81d2");
        }


        public static string GenerateSignature(Dictionary<string, string> parameters, string apiKey)
        {
            // Сортировка параметров по ключам в алфавитном порядке
            var sortedParameters = new SortedDictionary<string, string>(parameters);

            // Конкатенация значений параметров с разделителем |
            string concatenatedParameters = string.Join("|", sortedParameters.Values);

            // Преобразование API ключа в массив байтов
            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);

            // Хеширование строки параметров в SHA256
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedParameters));

                // Конкатенация хэша с API ключом
                byte[] concatenatedBytes = new byte[hashBytes.Length + apiKeyBytes.Length];
                Array.Copy(hashBytes, concatenatedBytes, hashBytes.Length);
                Array.Copy(apiKeyBytes, 0, concatenatedBytes, hashBytes.Length, apiKeyBytes.Length);

                // Хеширование второго уровня с использованием SHA256
                byte[] finalHash = sha256.ComputeHash(concatenatedBytes);

                // Преобразование хэша в строку в шестнадцатеричном формате
                return BitConverter.ToString(finalHash).Replace("-", "").ToLower();
            }
        }
    }
}