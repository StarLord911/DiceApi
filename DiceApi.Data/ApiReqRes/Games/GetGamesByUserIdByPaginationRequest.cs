using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    public class GetGamesByUserIdByPaginationRequest
    {
        public long Id { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
