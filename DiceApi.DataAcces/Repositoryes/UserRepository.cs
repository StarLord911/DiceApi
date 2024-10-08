﻿using Dapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Api;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
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

        public async Task<long> Add(UserRegistrationModel user)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var allUsers = GetAll();

                string query = @"INSERT INTO Users (name, password, ballance, ownerId, registrationDate, isActive, referalPercent, registrationIp, telegramUserId, enableWithdrawal)
                                    VALUES (@Name, @Password, @Ballance, @OwnerId, @RegistrationDate, @IsActive, @ReferalPercent, @RegistrationIp, @TelegramUserId, @EnableWithdrawal)
                                    SELECT CAST(SCOPE_IDENTITY() AS int)";

                var parameters = new
                {
                    Name = user.Name,
                    Password = user.Password, //хешировать
                    Ballance = 0,
                    OwnerId = user.OwnerId,
                    RegistrationDate = DateTime.UtcNow.GetMSKDateTime(),
                    IsActive = true,
                    ReferalPercent = 10,
                    RegistrationIp = user.IpAddres,
                    TelegramUserId = user.TelegramUserId,
                    EnableWithdrawal = true,
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
                        RegistrationDate = DateTime.UtcNow
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

        public async Task<GetPaginatedDataByUserIdResponce<User>> GetRefferalsByUserId(GetReferalsByUserIdRequest request)
        {
            var result = new GetPaginatedDataByUserIdResponce<User>();
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                int offset = (request.PageNumber - 1) * request.PageSize;

                var queryResult = (await db.QueryAsync<User>($@"SELECT * FROM Users WHERE isActive = 1 and ownerId = @ownerId 
                                ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                                new { ownerId = request.UserId, Offset = offset, PageSize = request.PageSize })).ToList();

                var countQuery = await db.ExecuteScalarAsync<int>($@"SELECT count(*) FROM Users WHERE isActive = 1 and ownerId = @ownerId", new { ownerId = request.UserId });
                var pageCont = (int)Math.Ceiling((double)countQuery / request.PageSize);

                result.PaginatedData = new PaginatedList<User>(queryResult, pageCont, request.PageNumber);

                return result;


            }
        }

        public async Task<PaginatedList<User>> GetUsersByPagination(GetUsersByPaginationRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var orderBy = "ORDER BY Id";

                if (request.OrderByBallance.HasValue && request.OrderByBallance == true)
                {
                    orderBy = "ORDER BY ballance";
                }

                string query = $"SELECT * FROM Users {orderBy} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                int offset = (request.PageNumber - 1) * request.PageSize;

                var countQuery = await db.ExecuteScalarAsync<int>($@"SELECT count(*) FROM Users");
                var pageCont = (int)Math.Ceiling((double)countQuery / request.PageSize);
                var users = await db.QueryAsync<User>(query, new { Offset = offset, PageSize = request.PageSize });

                return new PaginatedList<User>(users.ToList(), pageCont, request.PageNumber);
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

        public async Task<List<User>> GetRefferalsByUserId(long ownerId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<User>($@"SELECT * FROM Users WHERE isActive = 1 and ownerId = @ownerId",
                                new { ownerId })).ToList();
            }
        }

        public async Task<PaginatedList<User>> GetUsersByName(FindUserByNameRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                int offset = (request.Pagination.PageNumber - 1) * request.Pagination.PageSize;

                var queryResult = (await db.QueryAsync<User>($@"SELECT * FROM Users WHERE name like '%{request.Name}%' 
                                ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                                new { Offset = offset, PageSize = request.Pagination.PageSize })).ToList();

                var countQuery = await db.ExecuteScalarAsync<int>($@"SELECT count(*) FROM Users WHERE name like '%{request.Name}%'");
                var pageCont = (int)Math.Ceiling((double)countQuery / request.Pagination.PageSize);

                return new PaginatedList<User>(queryResult, pageCont, request.Pagination.PageNumber);
            }
        }

        public async Task UpdateUserInformation(UpdateUserRequest request)
        {
            var query = new StringBuilder("UPDATE Users SET ");
            var parameters = new DynamicParameters();

            if (request.Ballance.HasValue)
            {
                query.Append("ballance = @Ballance, ");
                parameters.Add("@Ballance", request.Ballance.Value);
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                query.Append("name = @Name, ");
                parameters.Add("@Name", request.Name);
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                query.Append("password = @Password, ");
                parameters.Add("@Password", request.Password);
            }

            if (request.ReffetalPercent != null)
            {
                query.Append("referalPercent = @ReferalPercent, ");
                parameters.Add("@ReferalPercent", request.ReffetalPercent.Value);
            }

            if (request.BlockUser != null)
            {
                query.Append("isActive = @BlockUser, ");
                parameters.Add("@BlockUser", !request.BlockUser.Value);
            }

            if (!string.IsNullOrEmpty(request.BlockReason))
            {
                query.Append("blockReason = @BlockReason, ");
                parameters.Add("@BlockReason", request.BlockReason);
            }

            if (request.PaymentForWithdrawal != null)
            {
                query.Append("paymentForWithdrawal = @PaymentForWithdrawal, ");
                parameters.Add("@PaymentForWithdrawal", request.PaymentForWithdrawal);
            }

            if (request.EnableWithdrawal != null)
            {
                query.Append("enableWithdrawal = @EnableWithdrawal, ");
                parameters.Add("@EnableWithdrawal", request.EnableWithdrawal);
            }

            query.Length -= 2;

            query.Append(" WHERE Id = @UserId;");
            parameters.Add("@UserId", request.UserId);

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(query.ToString(), parameters);
            }
        }

        public async Task<PaginatedList<UserPaymentInfo>> GetUserPaymentInfoByPagination(PaginationRequest request)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                // Выполнение SQL-запроса с использованием Dapper
                string sql = @"
                    SELECT u.id, u.name, SUM(p.Amount) AS TotalPaymentAmount
                    FROM Users u
                    LEFT JOIN Payments p ON u.id = p.UserId
                    WHERE p.Status = 'Payed'
                    GROUP BY u.id, u.name
                    ORDER BY TotalPaymentAmount DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                string countSql = @"
                    SELECT COUNT(*)
                    FROM (
                        SELECT u.id, u.name, SUM(p.Amount) AS TotalPaymentAmount
                        FROM Users u
                        LEFT JOIN Payments p ON u.id = p.UserId
                        WHERE p.Status = 'Payed'
                        GROUP BY u.id, u.name
                    ) AS subquery";

                // Выполнение SQL-запроса для получения количества записей
                int totalCount = await connection.ExecuteScalarAsync<int>(countSql);
                var pageCont = (int)Math.Ceiling((double)totalCount / request.PageSize);

                // Выполнение SQL-запроса с параметрами
                var queryResult = await connection.QueryAsync<UserPaymentInfo>(sql, new { Offset = (request.PageNumber - 1) * request.PageSize, PageSize = request.PageSize });

                return new PaginatedList<UserPaymentInfo>(queryResult.ToList(), pageCont, request.PageNumber);
            }
        }


        public async Task<PaginatedList<UserPaymentWithdrawalInfo>> GetUserPaymentWithdrawalInfoByPagination(PaginationRequest request)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                // Выполнение SQL-запроса с использованием Dapper
                string sql = @"
                    SELECT u.id, u.name, SUM(p.Amount) AS TotalPaymentWithdrawalAmount
                    FROM Users u
                    LEFT JOIN Withdrawal p ON u.id = p.UserId
                    WHERE p.Status = 2
                    GROUP BY u.id, u.name
                    ORDER BY TotalPaymentWithdrawalAmount DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                string countSql = @"
                    SELECT COUNT(*)
                    FROM (
                        SELECT u.id, u.name, SUM(p.Amount) AS TotalPaymentWithdrawalAmount
                        FROM Users u
                        LEFT JOIN Withdrawal p ON u.id = p.UserId
                        WHERE p.Status = 2
                        GROUP BY u.id, u.name
                    ) AS subquery";

                // Выполнение SQL-запроса для получения количества записей
                int totalCount = connection.ExecuteScalar<int>(countSql);
                var pageCont = (int)Math.Ceiling((double)totalCount / request.PageSize);

                // Выполнение SQL-запроса с параметрами
                var queryResult = await connection.QueryAsync<UserPaymentWithdrawalInfo>(sql, new { Offset = (request.PageNumber - 1) * request.PageSize, PageSize = request.PageSize });

                return new PaginatedList<UserPaymentWithdrawalInfo>(queryResult.ToList(), pageCont, request.PageNumber);
            }
        }

        public async Task<PaginatedList<UserRefferalInfo>> GetUserUserRefferalInfoByPagination(PaginationRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageSize", request.PageSize);
            parameters.Add("@PageNumber", request.PageNumber);
            parameters.Add("@Offset", (request.PageNumber - 1) * request.PageSize);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var totalRecords = connection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM Users");

                var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                var results = await connection.QueryAsync<UserRefferalInfo>(
                    @"SELECT u.id, u.name, COUNT(r.ownerId) AS ReferralCount, u.referalSum AS EarnedMoney
                    FROM Users u
                    LEFT JOIN Users r ON u.id = r.ownerId
                    GROUP BY u.id, u.name, u.referalSum
                    ORDER BY EarnedMoney DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                    parameters,
                    commandType: CommandType.Text
                );


                return new PaginatedList<UserRefferalInfo>(results.ToList(), totalPages, request.PageNumber);
            }
        }

        public async Task<int> GetUsersCount()
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT COUNT(*) FROM Users";
                return await connection.ExecuteScalarAsync<int>(query);
            }
        }

        public async Task<PaginatedList<UserMultyAccaunt>> GetMultyAccauntsByUserId(GetMultyAccauntsByUserIdRequest request)
        {
            var user = GetById(request.UserId);

            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                // Выполнение SQL-запроса с использованием Dapper
                string sql = $@"
                    SELECT *
                    FROM Users 
                    WHERE registrationIp = '{user.RegistrationIp}' and id != '{user.Id}'
                    ORDER BY id
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                string countSql = $@"SELECT COUNT(*) FROM Users where registrationIp = '{user.RegistrationIp}' and id != '{user.Id}'";

                // Выполнение SQL-запроса для получения количества записей
                int totalCount = connection.ExecuteScalar<int>(countSql);
                var pageCont = (int)Math.Ceiling((double)totalCount / request.Pagination.PageSize);

                // Выполнение SQL-запроса с параметрами
                var queryResult = await connection.QueryAsync<UserMultyAccaunt>(sql, new { Offset = (request.Pagination.PageNumber - 1) * request.Pagination.PageSize, PageSize = request.Pagination.PageSize });

                return new PaginatedList<UserMultyAccaunt>(queryResult.ToList(), pageCont, request.Pagination.PageNumber);
            }

        }

        public async Task UpdateAuthDateByUserId(long userId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = $"UPDATE Users SET lastAuthDate = @date where id = {userId}";
                await connection.ExecuteAsync(query, new { date = DateTime.UtcNow });
            }
        }

        public async Task UpdateAuthIpByUserId(long userId, string ip)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = $"UPDATE Users SET lastAuthIp = @ip where id = {userId}";
                await connection.ExecuteAsync(query, new { ip });
            }
        }

        public async Task DeleteUserById(long id)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = $"delete from Users where id = {id}";
                await connection.ExecuteAsync(query);
            }
        }

        public async Task UpdateRefferalOwnerId(long userId, long value)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = $@"update Users set ownerId = @newOwner where Id = @id";

                await db.ExecuteAsync(query, new { id = userId, newOwner = value });
            }
        }

        public async Task<User> GetUserByName(string name)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstAsync<User>("SELECT * FROM Users WHERE name = @name", new { name });
            }
        }

        public async Task<bool> IsTelegramUserRegistred(long telegramId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<User>("SELECT * FROM Users WHERE telegramUserId = @telegramId", new { telegramId })).Any();
            }
        }

        public async Task LinkTelegram(LinkTelegram linkTelegram)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = $@"update Users set telegramUserId = {linkTelegram.TelegramId} where Id = {linkTelegram.UserId}";
                
                await db.ExecuteAsync(query);
            }
        }

        public async Task<int> CheckUserAccaunt(long telegramId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = $"SELECT COUNT(*) FROM Users where telegramUserId = {telegramId}";
                return await connection.ExecuteScalarAsync<int>(query);
            }
        }
    }
}