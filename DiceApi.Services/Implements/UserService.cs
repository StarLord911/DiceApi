﻿using AutoMapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Api;
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

        public async Task<GetPainatidDataByUserIdResponce<UserReferral>> GetRefferalsByUserId(GetReferalsByUserIdRequest request)
        {
            var dbResult = await _userRepository.GetRefferalsByUserId(request);

            var res = new GetPainatidDataByUserIdResponce<UserReferral>();
            res.PaginatedData = new PaginatedList<UserReferral>(dbResult.PaginatedData.Items.Select(u => _mapper.Map<UserReferral>(u)).ToList(),
                dbResult.PaginatedData.TotalPages, dbResult.PaginatedData.PageIndex);

            return res;
        }

        public async Task<List<User>> GetUsersByPagination(GetUsersByPaginationRequest request)
        {
            return await _userRepository.GetUsersByPagination(request);
        }

        public async Task UpdateReferalSum(long userId, decimal sum)
        {
            await _userRepository.UpdateReferalSum(userId, sum);
        }
    }
}