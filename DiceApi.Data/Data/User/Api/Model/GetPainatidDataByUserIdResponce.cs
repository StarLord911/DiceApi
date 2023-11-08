using DiceApi.Data.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Api
{
    public class GetPainatidDataByUserIdResponce<T>
    {
        [JsonProperty("paginatedData")]
        public PaginatedList<T> PaginatedData { get; set; }

    }
}
