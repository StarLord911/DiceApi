using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос для поиска юзера по имени
    /// </summary>
    public class FindUserByNameRequest
    {
        public PaginationRequest Pagination { get; set; }

        public string Name { get; set; }
    }
}
