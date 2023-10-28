using DiceApi.Common;
using DiceApi.Data.Data.Payment;
using DiceApi.Services.Contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class PaymentAdapterService : IPaymentAdapterService
    {
        private readonly string merchantId;
        private readonly string secretKey = "your_secret_key";
        private readonly string _shopId = "your_secret_key";

        private double _currentBallance = -1;

        public PaymentAdapterService(IConfiguration configuration)
        {
            merchantId = configuration[ConfigerationNames.FreeKassaMerchantId];
        }

        public Task<string> CheckPaymentStatus(string paymentId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreatePaymentForm(CreatePaymentRequest createPaymentRequest)
        {
            string apiUrl = "https://pay.freekassa.ru/api/v1/create_payment";

            // Формируем параметры запроса
            var parameters = new Dictionary<string, string>
            {
                { "merchant_id", merchantId },
                { "shopId", _shopId },
                //{ "nonce", createPaymentRequest.PaymentId.ToString() },

                //{ "amount", createPaymentRequest.Amount.ToString() },
                //{ "order_id", createPaymentRequest.PaymentId.ToString()}
            };

            AddSignature(parameters);

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(apiUrl, new FormUrlEncodedContent(parameters));
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<double> GetCurrentBallance()
        {
            if (_currentBallance == -1)
            {
                _currentBallance = await GetCurrentBallanceFromAdapter();
            }
            //пока захаркодил, тут нужно получать текущий баланс
            return await Task.FromResult<double>(_currentBallance);
        }

        /// <summary>
        /// Обновление баланса
        /// </summary>
        /// <param name="sum"></param>
        public void UpdateCurrentBallance(double sum)
        {
            _currentBallance += sum;
        }

        private void AddSignature(Dictionary<string, string> parameters)
        {
            var sortedParameters = new SortedDictionary<string, string>(parameters);
            var signatureString = string.Join(":", sortedParameters.Values) + secretKey;

            using (var sha256 = SHA256.Create())
            {
                var signatureBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(signatureString));
                var signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

                parameters.Add("signature", signature);
            }
        }

        /// <summary>
        /// Запрос к палтежному адаптеру для получения баланса.
        /// </summary>
        /// <returns></returns>
        private Task<double> GetCurrentBallanceFromAdapter()
        {
            return Task.FromResult<double>(50000);
        }
    }
}