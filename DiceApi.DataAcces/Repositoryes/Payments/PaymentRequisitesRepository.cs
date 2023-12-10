using Dapper;
using DiceApi.Common;
using DiceApi.Data.Payments;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public class PaymentRequisitesRepository : IPaymentRequisitesRepository
    {

        private readonly string _connectionString;

        public PaymentRequisitesRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public async Task AddPaymentRequisite(PaymentRequisite paymentRequisite)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                INSERT INTO PaymentRequisites (UserId, FreeKassaNumber)
                VALUES (@UserId, @FreeKassaNumber)";

                await connection.ExecuteAsync(query, paymentRequisite);
            }
        }

        public async Task<PaymentRequisite> GetPaymentRequisiteByUserId(long userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM PaymentRequisites where UserId = {userId}";

                var queryResult = await connection.QueryAsync<PaymentRequisite>(query);

                if (queryResult.Any())
                {
                    return queryResult.FirstOrDefault();
                }

                return new PaymentRequisite();
            }
        }
    }
}
