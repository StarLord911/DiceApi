using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using DiceApi.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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


        private readonly HttpClient _httpClient;


        public PaymentAdapterService()
        {
            _httpClient = new HttpClient();
        }

        public Task<string> CheckPaymentStatus(string paymentId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreatePaymentForm(CreateOrderRequest createPaymentRequest)
        {
            string apiUrl = "https://api.freekassa.ru/v1/orders/create";

            string jsonRequest = JsonConvert.SerializeObject(createPaymentRequest);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            else
            {
                throw new ApplicationException($"Failed to create order. Status code: {response.StatusCode}");
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