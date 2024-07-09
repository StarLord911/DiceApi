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
        public async Task<string> CreatePaymentForm(CreatePaymentRequest request, long paymentId)
        {
            var createPaymentRequest = FreeKassHelper.CreateOrderRequest(request, paymentId);

            return (await RequestToFreeKassa<CreatePaymentResponce>("orders/create", createPaymentRequest)).Location;
        }

        public async Task<decimal> GetCurrentBallance()
        {
            if (_currentBallance == -1)
            {
                //_currentBallance = await GetBalanceAsync();
            }
            //пока захаркодил, тут нужно получать текущий баланс
            return await Task.FromResult<decimal>(50000);
        }

        /// <summary>
        /// Обновление баланса
        /// </summary>
        /// <param name="sum"></param>
        public void UpdateCurrentBallance(decimal sum)
        {
            _currentBallance += sum;
        }

        public async Task<List<FkWaletSpbWithdrawalBank>> GetSpbWithdrawalBanks()
        {
            return await GetSpbWithdrawalBanksRequestFkWalet();

        }

        public async Task<bool> CreateWithdrawal(Withdrawal withdrawal)
        {
            try
            {
                await CreateWithdrawalRequestFkWalet(withdrawal);
            }
            catch (Exception ex)
            {
                await _logRepository.LogError($"CreateWithdrawal error {withdrawal.Id} {ex.Message} {ex.StackTrace}");
                return false;
            }
            return true;
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
                    return (await RequestToFreeKassa<OrderResponse>("orders", request)).Orders.FirstOrDefault();
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

        private async Task<T> RequestToFreeKassa<T>(string url, string json)
        {
            string apiUrl = "https://api.freekassa.com/v1/" + url;

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, httpContent);

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var decodedJson = DecodeUnicode(jsonResponse);

            if (response.IsSuccessStatusCode)
            {
                await _logRepository.LogInfo("CreatePaymentForm" + DecodeUnicode(jsonResponse));
                return SerializationHelper.Deserialize<T>(jsonResponse);
            }
            else
            {
                await _logRepository.LogError($"Error when create payment form {decodedJson}");
                throw new ApplicationException($"Failed to create order. Status code: {response.StatusCode} Reason: {decodedJson}");
            }
        }

        private async Task<List<FkWaletSpbWithdrawalBank>> GetSpbWithdrawalBanksRequestFkWalet()
        {
            var signWithBody = CalculateSHA256Hash(_walletPrivateKey);

            var responce = await SendRequest("https://api.fkwallet.io/v1/b5d5b4c85a3bf3147e44303c835d0c9c/sbp_list", null , signWithBody, HttpMethod.Get);
            var result = SerializationHelper.Deserialize<FkWaletSpbWithdrawalBankResponce>(responce);

            return result.GetSpbWithdrawalBanks;
        }


        private async Task CreateWithdrawalRequestFkWalet(Withdrawal withdrawal)
        {
            var dataWithBody = FreeKassHelper.GetWithdrawalRequest(withdrawal);
            var jsonDataWithBody = JsonConvert.SerializeObject(dataWithBody);
            var signWithBody = CalculateSHA256Hash(jsonDataWithBody + _walletPrivateKey);

            await SendRequest("https://api.fkwallet.io/v1/b5d5b4c85a3bf3147e44303c835d0c9c/withdrawal", jsonDataWithBody, signWithBody, HttpMethod.Post);
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