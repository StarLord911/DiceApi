using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiModels
{
    /// <summary>
    /// Модель для запросов с пагинацией.
    /// </summary>
    public class PaginationRequest
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
