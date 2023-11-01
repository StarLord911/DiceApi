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
    }
}
