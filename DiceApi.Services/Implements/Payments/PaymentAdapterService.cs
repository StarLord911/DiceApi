using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Data.Payment;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class PaymentAdapterService : IPaymentAdapterService
    {
        private decimal _currentBallance = -1;
        private string _walletPrivateKey = "wdtXREmm0ru0j5k55UqPUJfKleyZKy1nXfpAbNcsyQhckDyHf4";


        private readonly HttpClient _httpClient;

        private readonly ILogRepository _logRepository;

        public PaymentAdapterService(ILogRepository logRepository)
        {
            _httpClient = new HttpClient();
            _logRepository = logRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createPaymentRequest"></param>
        /// <returns></returns>
        public async Task<CreatePaymentResponse> CreatePaymentForm(CreatePaymentRequest request, long paymentId)
        {
            var createPaymentRequest = FreeKassHelper.CreateOrderRequest(request, paymentId);

            string apiUrl = "https://api.freekassa.com/v1/orders/create";

            var httpContent = new StringContent(createPaymentRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, httpContent);

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var decodedJson = DecodeUnicode(jsonResponse);

            if (response.IsSuccessStatusCode)
            {
                await _logRepository.LogInfo("CreatePaymentForm" + DecodeUnicode(jsonResponse));
                var result = SerializationHelper.Deserialize<CreateFreeKassaPaymentResponce>(jsonResponse);

                return new CreatePaymentResponse
                {
                    Succesful = true,
                    Message = null,
                    Location = result.Location,
                    OrderId = result.OrderId
                };
            }
            else
            {
                await _logRepository.LogError($"Error when create payment form {decodedJson}");

                return new CreatePaymentResponse
                {
                    Succesful = false,
                    Message = decodedJson,
                    Location = null
                };
            }

        }

        public async Task<decimal> GetCurrentBallance()
        {
           var signWithBody = CalculateSHA256Hash(_walletPrivateKey);

            var responce = await SendRequest("https://api.fkwallet.io/v1/b5d5b4c85a3bf3147e44303c835d0c9c/balance", null, signWithBody, HttpMethod.Get);
            var result = SerializationHelper.Deserialize<FkWaletBallanceApiResponse>(responce);

            return result.Data.FirstOrDefault(b => b.CurrencyCode == "RUB").Value;
        }


        public async Task<List<FkWaletSpbWithdrawalBank>> GetSpbWithdrawalBanks()
        {
            var signWithBody = CalculateSHA256Hash(_walletPrivateKey);

            var responce = await SendRequest("https://api.fkwallet.io/v1/b5d5b4c85a3bf3147e44303c835d0c9c/sbp_list", null, signWithBody, HttpMethod.Get);
            var result = SerializationHelper.Deserialize<FkWaletSpbWithdrawalBankResponce>(responce);

            return result.GetSpbWithdrawalBanks;
        }

        public async Task<FkWaletWithdrawalStatusResponce> CreateWithdrawal(Withdrawal withdrawal)
        {
            try
            {
                var dataWithBody = FreeKassHelper.GetWithdrawalRequest(withdrawal);
                var jsonDataWithBody = JsonConvert.SerializeObject(dataWithBody);
                var signWithBody = CalculateSHA256Hash(jsonDataWithBody + _walletPrivateKey);

                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.fkwallet.io/v1/b5d5b4c85a3bf3147e44303c835d0c9c/withdrawal");

                    if (!string.IsNullOrEmpty(jsonDataWithBody))
                    {
                        request.Content = new StringContent(jsonDataWithBody, Encoding.UTF8, "application/json");
                    }

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", signWithBody);

                    var response = await client.SendAsync(request);

                    var rrr = DecodeUnicode(await response.Content.ReadAsStringAsync());

                    return SerializationHelper.Deserialize<FkWaletWithdrawalStatusResponce>(DecodeUnicode(await response.Content.ReadAsStringAsync()));
                }
            }
            catch (Exception ex)
            {
                await _logRepository.LogError($"CreateWithdrawal error {withdrawal.Id} {ex.Message} {ex.StackTrace}");
                throw;
            }
        }

        public async Task<GetFreeKassaOrderByIdResponce> GetOrderByFreeKassaId(long orderId)
        {
            var request = FreeKassHelper.GetOrderByIdRequest(orderId);

            int maxRetries = 3;
            Exception lastException = null;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    var apiUrl = "https://api.freekassa.com/v1/orders";
                    var httpContent = new StringContent(request, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, httpContent);

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var decodedJson = DecodeUnicode(jsonResponse);

                    if (response.IsSuccessStatusCode)
                    {
                        await _logRepository.LogInfo("CreatePaymentForm" + DecodeUnicode(jsonResponse));
                        return SerializationHelper.Deserialize<OrderResponse>(jsonResponse).Orders.FirstOrDefault();
                    }
                    else
                    {
                        await _logRepository.LogError($"Error when create payment form {decodedJson}");
                        throw new ApplicationException($"Failed to create order. Status code: {response.StatusCode} Reason: {decodedJson}");
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    await Task.Delay(3000);
                }
            }

            // Если все попытки завершились неудачно, выбрасываем последнее исключение
            throw lastException;
        }

        public async Task<FkWaletWithdrawalStatusResponce> GetWithdrawalStatusFkWalet(long fkWaletId)
        {
            var signWithBody = CalculateSHA256Hash(_walletPrivateKey);

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.fkwallet.io/v1/b5d5b4c85a3bf3147e44303c835d0c9c/withdrawal/{fkWaletId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", signWithBody);

                var response = await client.SendAsync(request);
                var rr = DecodeUnicode(await response.Content.ReadAsStringAsync());
                return SerializationHelper.Deserialize<FkWaletWithdrawalStatusResponce>(await response.Content.ReadAsStringAsync());
            }

        }

        private async Task<string> SendRequest(string url, string jsonData, string sign, HttpMethod method)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(method, url);

                if (!string.IsNullOrEmpty(jsonData))
                {
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sign);

                var response = await client.SendAsync(request);
                return DecodeUnicode(await response.Content.ReadAsStringAsync());
            }
        }


        private string DecodeUnicode(string value)
        {
            return Regex.Unescape(value);
        }

        private string CalculateSHA256Hash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}