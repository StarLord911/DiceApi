using Dapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Api;
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

                string query = @"INSERT INTO Users (name, password, ballance, ownerId, registrationDate, isActive, referalPercent )
                                    VALUES (@Name, @Password, @Ballance, @OwnerId, @RegistrationDate, @IsActive, @ReferalPercent)
                                    SELECT CAST(SCOPE_IDENTITY() AS int)";

                var parameters = new
                {
                    Name = user.Name,
                    Password = HashHelper.GetSHA256Hash(user.Password), //хешировать
                    Ballance = 0,
                    OwnerId = user.OwnerId,
                    RegistrationDate = DateTime.Now,
                    IsActive = true,
                    ReferalPercent = 10
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

        public async Task<GetPainatidDataByUserIdResponce<User>> GetRefferalsByUserId(GetReferalsByUserIdRequest request)
        {
            var result = new GetPainatidDataByUserIdResponce<User>();
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                int offset = (request.PageNumber - 1) * request.PageSize;

                var queryResult = (await db.QueryAsync<User>($@"SELECT * FROM Users WHERE isActive = 1 and ownerId = @ownerId 
                                ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                                new { ownerId = request.Id, Offset = offset, PageSize = request.PageSize })).ToList();

                var countQuery = await db.ExecuteScalarAsync<int>($@"SELECT count(*) FROM Users WHERE isActive = 1 and ownerId = @ownerId", new { ownerId = request.Id });
                var pageCont = (int)Math.Ceiling((double)countQuery / request.PageSize);

                result.PaginatedData = new PaginatedList<User>(queryResult, pageCont, request.PageNumber);

                return result;

               
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

        public async Task UpdateReferalSum(long userId, decimal sum)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = $@"update Users set referalSum = @sum where Id = @id";

                await db.ExecuteAsync(query, new { id = userId, sum });
            }
        }
    }
}