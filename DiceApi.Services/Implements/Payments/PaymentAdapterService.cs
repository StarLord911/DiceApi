using DiceApi.Common;
using DiceApi.Data;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class PaymentAdapterService : IPaymentAdapterService
    {
        private decimal _currentBallance = -1;
        private readonly string _apiUrl = "https://api.freekassa.ru/v1/";


        private readonly HttpClient _httpClient;

        private readonly ILogRepository _logRepository;

        public PaymentAdapterService(ILogRepository logRepository)
        {
            _httpClient = new HttpClient();
            _logRepository = logRepository;
        }

        public Task<string> CheckPaymentStatus(string paymentId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createPaymentRequest"></param>
        /// <returns></returns>
        public async Task<string> CreatePaymentForm(CreateOrderRequest createPaymentRequest)
        {
            string apiUrl = "https://api.freekassa.ru/v1/orders/create";

            string jsonRequest = JsonConvert.SerializeObject(createPaymentRequest);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("Authorization", "cb67a126d4bc65972b2ed97d42c9a045");


            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                await _logRepository.LogInfo("CreatePaymentForm" + jsonResponse);
                using (JsonTextReader reader = new JsonTextReader(new StringReader(jsonResponse)))
                {
                    // Создание экземпляра JsonSerializer
                    JsonSerializer serializer = new JsonSerializer();

                    // Десериализация JSON и приведение результата к типу Person
                    var paymentResponse = serializer.Deserialize<CreatePaymentResponce>(reader);
                    return paymentResponse.Location;
                }
            }
            else
            {
                throw new ApplicationException($"Failed to create order. Status code: {response.StatusCode}");
            }
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

        /// <summary>
        /// Запрос к палтежному адаптеру для получения баланса.
        /// </summary>
        /// <returns></returns>
        private async Task<decimal> GetBalanceAsync()
        {

                //$sign = md5($merchant_id.':'.$_REQUEST['AMOUNT'].':'.$merchant_secret.':'.$_REQUEST['MERCHANT_ORDER_ID']);

            int nonce = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var _shopId = int.Parse(ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaShopId));

            var data = new Dictionary<string, object>()
            {
                { "shopId", _shopId },
                { "nonce", nonce }
            };

            var sortedData = new SortedDictionary<string, object>(data);
            var signString = string.Join("|", sortedData.Values);
            var apiKey = ConfigHelper.GetConfigValue(ConfigerationNames.FreeKassaSecretOne);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey)))
            {
                var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));
                var signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

                sortedData.Add("signature", signature);
            }

            string json = JsonConvert.SerializeObject(sortedData);

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{_apiUrl}balance", new StringContent(json, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    await _logRepository.LogInfo("CreatePaymentForm" + responseContent);

                    var balanceResponse = JsonConvert.DeserializeObject<BalanceResponse>(responseContent);

                    if (balanceResponse.Type == "success")
                    {
                        return 0;
                        //return balanceResponse.Ballance;
                    }
                    else
                    {
                        await _logRepository.LogInfo("Error GetBalanceAsync" + balanceResponse.Message);

                        throw new Exception(balanceResponse.Message);
                    }
                }
                else
                {
                    await _logRepository.LogInfo("Error GetBalanceAsync" + response.StatusCode);

                    throw new Exception("An error occurred while performing the API request.");
                }
            }
        }



    }
}