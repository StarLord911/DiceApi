using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    public class GetMultyAccauntsByUserIdRequest
    {
        public long UserId { get; set; }

        public PaginationRequest Pagination { get; set; }
    }
}
