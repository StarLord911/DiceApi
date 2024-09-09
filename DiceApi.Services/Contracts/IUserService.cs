using DiceApi.Data;
using DiceApi.Data.Api;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest model);

        Task<AuthenticateResponse> Register(UserRegistrationModel userModel);

        Task<bool> IsTelegramUserRegistred(long telegramId);

        IEnumerable<User> GetAll();

        User GetById(long id);

        Task<User> GetUserByName(string name);

        Task UpdateUserBallance(long userId, decimal sum);

        Task AddUserBallance(long userId, decimal sum);


        Task<GetPaginatedDataByUserIdResponce<UserReferral>> GetRefferalsByUserId(GetReferalsByUserIdRequest request);

        Task<List<User>> GetRefferalsByUserId(long ownerId);

        Task<PaginatedList<User>> GetUsersByPagination(GetUsersByPaginationRequest request);

        Task UpdateReferalSum(long userId, decimal sum);

        Task<PaginatedList<User>> GetUsersByName(FindUserByNameRequest request);

        Task UpdateUserInformation(UpdateUserRequest request);

        Task<PaginatedList<UserPaymentInfo>> GetUserPaymentInfoByPagination(PaginationRequest request);

        Task<PaginatedList<UserPaymentWithdrawalInfo>> GetUserPaymentWithdrawalInfoByPagination(PaginationRequest request);
        Task<PaginatedList<UserRefferalInfo>> GetUserUserRefferalInfoByPagination(PaginationRequest request);
        Task<int> GetUserCount();
        Task<PaginatedList<UserMultyAccaunt>> GetMultyAccauntsByUserId(GetMultyAccauntsByUserIdRequest request);
        Task DeleteUserById(long id);
        Task UpdateRefferalOwnerId(long userId, long value);
    }
}
