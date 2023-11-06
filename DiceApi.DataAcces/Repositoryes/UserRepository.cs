using Dapper;
using DiceApi.Common;
using DiceApi.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public List<User> GetAll()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<User>("SELECT * FROM Users").ToList();
            }
        }

        public User GetById(long id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<User>("SELECT * FROM Users WHERE Id = @id", new { id }).FirstOrDefault();
            }
        }

        public async Task<long> Add(UserRegisterResponce user)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var allUsers = GetAll();

                if (allUsers.Any(u => u.Name == user.Name))
                {
                    throw new Exception("Is user contains");
                }

                string query = @"INSERT INTO Users (name, password, ballance, ownerId, registrationDate, isActive )
                                    VALUES (@Name, @Password, @Ballance, @OwnerId, @RegistrationDate, @IsActive)
                                    SELECT CAST(SCOPE_IDENTITY() AS int)";

                var parameters = new
                {
                    Name = user.Name,
                    Password = HashHelper.GetSHA256Hash(user.Password), //хешировать
                    Ballance = 0,
                    OwnerId = user.OwnerId,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                };

                var userId = await db.ExecuteScalarAsync<long>(query, parameters);

                if (user.OwnerId != null && user.OwnerId.HasValue && user.OwnerId.Value != 0)
                {
                    var invationsAddQuery = @"INSERT INTO Referrals  (referralID, senderID, registrationDate ) 
                                                VALUES(@ReferralID, @SenderID, @RegistrationDate)";

                    var invationsAddQueryParameters = new
                    {
                        ReferralID = userId,
                        SenderID = user.OwnerId.Value,
                        RegistrationDate = DateTime.Now
                    };

                    await db.ExecuteScalarAsync<long>(invationsAddQuery, invationsAddQueryParameters);
                }

                return userId;
            }
        }

        public async Task UpdateUserBallance(long userId, decimal newBallance)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = $@"update Users set ballance = @newBallance where Id = @id";

                await db.ExecuteAsync(query, new { id = userId, newBallance });
            }
        }

        public async Task<List<User>> GetRefferalsByUserId(long ownerId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<User>("SELECT * FROM Users WHERE isActive = 1 and ownerId = @ownerId", new { ownerId })).ToList();
            }
        }

        public async Task<List<User>> GetUsersByPagination(GetUsersByPaginationRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Users ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                int offset = (request.PageNumber - 1) * request.PageSize;

                var users = await db.QueryAsync<User>(query, new { Offset = offset, PageSize = request.PageSize });

                return users.ToList();
            }
        }
    }
}