using AutoMapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Api;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.DataAcces.Repositoryes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogRepository _logRepository;
        private readonly IMapper _mapper;


        public UserService(IUserRepository userRepository,
            ILogRepository logRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _logRepository = logRepository;
            _mapper = mapper;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var user = _userRepository
                .GetAll()
                .FirstOrDefault(x => x.Name == model.Name && x.Password == model.Password);

            if (user == null)
            {
                // todo: need to add logger
                await _logRepository.LogError($"Cannot find user by name: {model.Name}");
                return new AuthenticateResponse() { Info = "Cannot find user" };
            }

            if (user != null && user.IsActive == false)
            {
                // todo: need to add logger
                await _logRepository.LogError($"User is blocked {model.Name}");
                return new AuthenticateResponse() {Id = user.Id, Name = user.Name, Info = "User is blocked" };
            }

            var token = AuthHelper.GenerateJwtToken(user);
            await _userRepository.UpdateAuthDateByUserId(user.Id);
            await _userRepository.UpdateAuthIpByUserId(user.Id, model.AuthIpAddres);

            return new AuthenticateResponse(user, token) {Info = "Authenticate succes" };
        }

        public async Task<AuthenticateResponse> Register(UserRegistrationModel userModel)
        {
            try
            {
                await _userRepository.Add(userModel);

                var response = await Authenticate(new AuthenticateRequest
                {
                    Name = userModel.Name,
                    Password = userModel.Password
                });

                await _logRepository.LogInfo($"Register new user by name {userModel.Name}");

                return response;

            }
            catch (Exception ex)
            {
                await _logRepository.LogException("Error when Register", ex);
            }

            return new AuthenticateResponse() { Info = "Is user contains" };
        }

        public IEnumerable<User> GetAll()
        {
            return _userRepository.GetAll();
        }

        public User GetById(long id)
        {
            return _userRepository.GetById(id);
        }

        public async Task UpdateUserBallance(long userId, decimal sum)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return;
            }

            await _logRepository.LogInfo($"UpdateUserBallance new ballance: {sum} old ballance: {user.Ballance} userId: {userId} ");

            await _userRepository.UpdateUserBallance(userId, sum);
        }

        public async Task<GetPaginatedDataByUserIdResponce<UserReferral>> GetRefferalsByUserId(GetReferalsByUserIdRequest request)
        {
            var dbResult = await _userRepository.GetRefferalsByUserId(request);

            var res = new GetPaginatedDataByUserIdResponce<UserReferral>();
            res.PaginatedData = new PaginatedList<UserReferral>(dbResult.PaginatedData.Items.Select(u => _mapper.Map<UserReferral>(u)).ToList(),
                dbResult.PaginatedData.TotalPages, dbResult.PaginatedData.PageIndex);

            return res;
        }

        public async Task<PaginatedList<User>> GetUsersByPagination(GetUsersByPaginationRequest request)
        {
            return await _userRepository.GetUsersByPagination(request);
        }

        public async Task UpdateReferalSum(long userId, decimal sum)
        {
            await _userRepository.UpdateReferalSum(userId, sum);
        }

        public async Task<List<User>> GetRefferalsByUserId(long ownerId)
        {
            return await _userRepository.GetRefferalsByUserId(ownerId);
        }

        public async Task<PaginatedList<User>> GetUsersByName(FindUserByNameRequest request)
        {
            return await _userRepository.GetUsersByName(request);
        }

        public async Task UpdateUserInformation(UpdateUserRequest request)
        {
            await _userRepository.UpdateUserInformation(request);
        }

        public async Task<PaginatedList<UserPaymentInfo>> GetUserPaymentInfoByPagination(PaginationRequest request)
        {
            return await _userRepository.GetUserPaymentInfoByPagination(request);
        }

        public async Task<PaginatedList<UserPaymentWithdrawalInfo>> GetUserPaymentWithdrawalInfoByPagination(PaginationRequest request)
        {
            return await _userRepository.GetUserPaymentWithdrawalInfoByPagination(request);
        }

        public async Task<PaginatedList<UserRefferalInfo>> GetUserUserRefferalInfoByPagination(PaginationRequest request)
        {
            return await _userRepository.GetUserUserRefferalInfoByPagination(request);
        }

        public async Task<int> GetUserCount()
        {
            return await _userRepository.GetUsersCount();
        }

        public async Task<PaginatedList<UserMultyAccaunt>> GetMultyAccauntsByUserId(GetMultyAccauntsByUserIdRequest request)
        {
            return await _userRepository.GetMultyAccauntsByUserId(request);
        }

        public async Task DeleteUserById(long id)
        {
            await _userRepository.DeleteUserById(id);
        }

        public async Task UpdateRefferalOwnerId(long userId, long value)
        {
            await _userRepository.UpdateRefferalOwnerId(userId, value);
        }

        public async Task<User> GetUserByName(string name)
        {
            return await _userRepository.GetUserByName(name);
        }

        public async Task<bool> IsTelegramUserRegistred(long telegramId)
        {
            return await _userRepository.IsTelegramUserRegistred(telegramId);
        }

        public async Task AddUserBallance(long userId, decimal sum)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return;
            }

            await _logRepository.LogInfo($"UpdateUserBallance new ballance: {sum} old ballance: {user.Ballance} userId: {userId} ");

            await _userRepository.UpdateUserBallance(userId, user.Ballance + sum);
        }

        public async Task<string> LinkTelegram(LinkTelegram linkTelegram)
        {
            var user = _userRepository.GetById(linkTelegram.UserId);

            if (user.TelegramUserId != null && user.TelegramUserId != 0)
            {
                return "Этот аккаунт уже привязан";
            }

            if (await _userRepository.CheckUserAccaunt(linkTelegram.TelegramId) > 0)
            {
                return "Этот телеграм привязан к другому аккаунту";
            }

            await _userRepository.LinkTelegram(linkTelegram);

            return "Аккаунт успешно привязан";

        }
    }
}