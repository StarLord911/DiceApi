using Dapper;
using DiceApi.Common;
using DiceApi.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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

                string insertQuery = $@"INSERT INTO Withdrawal (UserId, Amount, CardNumber, CreateDate, IsActive) 
                                            VALUES (@UserId, @Amount, @CardNumber, @CreateDate, @IsActive)";

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

                string query = $@"Select * From Withdrawal where IsActive = 1";

                return (await connection.QueryAsync<Withdrawal>(query)).ToList();

            }
        }

        public async Task<List<Withdrawal>> GetAllByUserId(long userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"Select * From Withdrawal where IsActive = 1 and UserId = {userId}";

                return (await connection.QueryAsync<Withdrawal>(query)).ToList();

            }
        }

        public async Task<Withdrawal> GetById(long id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"Select * From Withdrawal where IsActive = 1 and Id = {id}";

                return (await connection.QueryAsync<Withdrawal>(query)).FirstOrDefault();

            }
        }

        public async Task UpdateIsActive(long id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = $@"update Withdrawal set IsActive = 0 where Id = {id}";

                await connection.QueryAsync<Withdrawal>(query);

            }
        }
    }
}
