﻿using DiceApi.Data;
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

        Task<AuthenticateResponse> Register(UserRegister userModel);

        IEnumerable<User> GetAll();

        User GetById(long id);

        Task UpdateUserBallance(long userId, double sum);

        Task UpdateUserPromoBallance(long userId, double sum);

    }
}
