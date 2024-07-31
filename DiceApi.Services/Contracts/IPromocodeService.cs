using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Promocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IPromocodeService
    {
        Task<string> CreatePromocode(CreatePromocodeRequest request);

        Task<ActivatePromocodeResponce> ActivetePromocode(ActivatePromocodeRequest request);

        Task<PaginatedList<PromocodeApiModel>> GetPromocodesByPagination(GetPromocodesByPaginationRequest request);

        Task<PaginatedList<PromocodeApiModel>> GetPromocodeByNameByLike(GetPromocodesByNameRequest request);

        Task<GenerateRefferalPromocodeResponce> CreateRefferalPromocode(GenerateRefferalPromocodeRequest request);

        Task<List<RefferalPromocode>> GetRefferalPromocodesByUserId(long userId);
    }
}
