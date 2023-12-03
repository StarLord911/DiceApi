using DiceApi.Data.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    public class GetWithdrawalsRequest
    {
        public PaginationRequest Pagination { get; set; }

        public bool OnlyActiveWithdrawals { get; set; }
    }
}
