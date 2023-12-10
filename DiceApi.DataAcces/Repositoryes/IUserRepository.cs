using DiceApi.Data;
using DiceApi.Data.Api;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IUserRepository
    {
        Task<long> Add(UserRegistrationModel user);

        User GetById(long id);
        
        List<User> GetAll();

        Task UpdateUserBallance(long userId, decimal newBallance);

        Task<GetPaginatedDataByUserIdResponce<User>> GetRefferalsByUserId(GetReferalsByUserIdRequest request);

        Task<PaginatedList<User>> GetUsersByPagination(GetUsersByPaginationRequest request);

        Task UpdateReferalSum(long userId, decimal sum);

        Task<List<User>> GetRefferalsByUserId(long ownerId);

        Task<PaginatedList<User>> GetUsersByName(FindUserByNameRequest request);
        Task UpdateUserInformation(UpdateUserRequest request);

        Task<PaginatedList<UserPaymentInfo>> GetUserPaymentInfoByPagination(PaginationRequest request);

        Task<PaginatedList<UserPaymentWithdrawalInfo>> GetUserPaymentWithdrawalInfoByPagination(PaginationRequest request);
        Task<PaginatedList<UserRefferalInfo>> GetUserUserRefferalInfoByPagination(PaginationRequest request);
        Task<int> GetUsersCount();
        Task<PaginatedList<UserMultyAccaunt>> GetMultyAccauntsByUserId(GetMultyAccauntsByUserIdRequest request);
    }
}
