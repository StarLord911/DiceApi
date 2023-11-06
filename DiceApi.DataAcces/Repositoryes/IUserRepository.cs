﻿using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IUserRepository
    {
        Task<long> Add(UserRegisterResponce user);

        User GetById(long id);
        
        List<User> GetAll();

        Task UpdateUserBallance(long userId, decimal newBallance);

        Task<List<User>> GetRefferalsByUserId(long ownerId);

        Task<List<User>> GetUsersByPagination(GetUsersByPaginationRequest request);
    }
}
