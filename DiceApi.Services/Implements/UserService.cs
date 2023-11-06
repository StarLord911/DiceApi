using DiceApi.Common;
using DiceApi.Data;
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

        public UserService(IUserRepository userRepository,
            ILogRepository logRepository)
        {
            _userRepository = userRepository;
            _logRepository = logRepository;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var user = _userRepository
                .GetAll()
                .FirstOrDefault(x => x.Name == model.Name && x.Password == HashHelper.GetSHA256Hash(model.Password));

            if (user == null)
            {
                // todo: need to add logger
                await _logRepository.LogError($"Cannot find user by name: {model.Name}");
                return null;
            }

            var token = AuthHelper.GenerateJwtToken(user);

            return new AuthenticateResponse(user, token) {Info = "Authenticate succes" };
        }

        public async Task<AuthenticateResponse> Register(UserRegisterResponce userModel)
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
                await _logRepository.LogException(ex);
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

            await _userRepository.UpdateUserBallance(userId, sum);
        }

        public async Task<List<User>> GetRefferalsByUserId(long id)
        {
            return await _userRepository.GetRefferalsByUserId(id);
        }

        public async Task<List<User>> GetUsersByPagination(GetUsersByPaginationRequest request)
        {
            return await _userRepository.GetUsersByPagination(request);
        }
    }
}