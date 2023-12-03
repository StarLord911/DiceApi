using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос для получения промокодов по названию
    /// </summary>
    public class GetPromocodesByNameRequest
    {
        public PaginationRequest Pagination { get; set; }

        public string Name { get; set; }
    }
}
