using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Получение промокодов по пагинаций.
    /// </summary>
    public class GetPromocodesByPaginationRequest
    {
        public PaginationRequest Pagination { get; set; }

        public bool OnlyActivePromocodes { get; set; }
    }
}
