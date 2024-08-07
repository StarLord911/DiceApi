using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Common
{
    public static class RatesHelper
    {
        public static decimal GetRates() 
        {
            string apiUrl = "https://api.coingecko.com/api/v3/simple/price?ids=tether&vs_currencies=rub";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response =  client.GetAsync(apiUrl).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;

                JObject json = JObject.Parse(responseBody);
                return  json["tether"]["rub"].Value<decimal>();
            }
        }
    }
}
