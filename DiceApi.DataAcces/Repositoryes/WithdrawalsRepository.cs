﻿using Dapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public class WithdrawalsRepository : IWithdrawalsRepository
    {
        private readonly string _connectionString;

        public WithdrawalsRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public async Task AddWithdrawal(Withdrawal withdrawal)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string insertQuery = $@"INSERT INTO Withdrawal (UserId, Amount, CardNumber, CreateDate, Status) 
                                            VALUES (@UserId, @Amount, @CardNumber, @CreateDate, @Status)";

                await connection.ExecuteAsync(insertQuery, withdrawal);

            }
        }

        public async Task<List<Withdrawal>> GetAll()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"Select * From Withdrawal";

                return (await connection.QueryAsync<Withdrawal>(query)).ToList();

            }
        }

        public async Task<List<Withdrawal>> GetAllActive()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"Select * From Withdrawal where Status = 1";

                return (await connection.QueryAsync<Withdrawal>(query)).ToList();

            }
        }

        public async Task<List<Withdrawal>> GetAllActiveByUserId(long userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"Select * From Withdrawal where Status = 1 and UserId = {userId}";

                return (await connection.QueryAsync<Withdrawal>(query)).ToList();

            }
        }

        public async Task<Withdrawal> GetById(long id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"Select * From Withdrawal where Id = {id}";

                return (await connection.QueryAsync<Withdrawal>(query)).FirstOrDefault();

            }
        }

        public async Task<List<Withdrawal>> GetByUserIdAll(long userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"Select * From Withdrawal where UserId = {userId}";

                return (await connection.QueryAsync<Withdrawal>(query)).ToList();

            }
        }

        public async Task DeactivateWithdrawal(long id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"update Withdrawal set Status = 2 where Id = {id}";

                await connection.QueryAsync<Withdrawal>(query);

            }
        }

        public async Task<PaginatedList<Withdrawal>> GetPaginatedWithdrawals(GetPaymentWithdrawalsRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = new StringBuilder();

                DynamicParameters parameters = new DynamicParameters();

                if (request.StartDate != null)
                {
                    query.Append($"CreateDate > @StartDate");
                    parameters.Add("@StartDate", request.StartDate);
                }

                if (request.EndDate != null)
                {
                    if (query.Length > 0)
                    {
                        query.Append(" AND");
                    }
                    query.Append($" CreateDate < @EndDate");
                    parameters.Add("@EndDate", request.EndDate);
                }

                if (request.Status != null)
                {
                    if (query.Length > 0)
                    {
                        query.Append(" AND");
                    }
                    query.Append($" Status = @Status");
                    parameters.Add("@Status", request.Status);
                }

                if (!string.IsNullOrEmpty(query.ToString()))
                {
                    query.Insert(0, "where ");
                }

                parameters.Add("@Offset", (request.Pagination.PageNumber - 1) * request.Pagination.PageSize);
                parameters.Add("@PageSize", request.Pagination.PageSize);

                var countQuery = $"SELECT COUNT(*) FROM Withdrawal {query.ToString()}";
                var paymentsQuery = $"SELECT * FROM Withdrawal {query.ToString()} ORDER BY CreateDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                int totalCount = await db.ExecuteScalarAsync<int>(countQuery, parameters);
                var withdrawals = await db.QueryAsync<Withdrawal>(paymentsQuery, parameters);
                var pageCount = (int)Math.Ceiling((double)totalCount / request.Pagination.PageSize);

                return new PaginatedList<Withdrawal>(withdrawals.ToList(), pageCount, request.Pagination.PageNumber);
            }
        }
    }
}
