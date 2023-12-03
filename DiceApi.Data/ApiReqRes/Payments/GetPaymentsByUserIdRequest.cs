using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос для получение пополнений юзера.
    /// </summary>
    public class GetPaymentsByUserIdRequest
    {
        public long UserId { get; set; }

        public PaginationRequest Pagination { get; set; }
    }
}
