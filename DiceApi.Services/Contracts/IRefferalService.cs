using DiceApi.Data;
using DiceApi.Data.Data.User.Api.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IRefferalService
    {
        public Task<GetReferalStatsResponce> GetReferalStats(GetRefferalStatsByUserIdRequest request);
    }
}
