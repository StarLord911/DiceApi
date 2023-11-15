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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public async Task<long> CreatePayment(Payment payment)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
               
                string query = @"
                    INSERT INTO Payments (orderId, amount, createdAt, status, userId)
                    VALUES (@OrderId, @Amount, @CreatedAt, @Status, @UserId)
                    SELECT CAST(SCOPE_IDENTITY() AS int)";

                var parameters = new
                {
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    CreatedAt = payment.CreatedAt,
                    Status = payment.Status,
                    UserId = payment.UserId
                };

                return await connection.ExecuteScalarAsync<long>(query, parameters);
            }
        }

        public async Task<List<Payment>> GetAllPayedPayments()
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"SELECT * FROM Payments WHERE Status = '{PaymentStatus.Payed}'";

                return (await connection.QueryAsync<Payment>(query)).ToList();
            }
        }

        public async Task<Payment> GetPaymentsById(long paymentId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Payments WHERE Id = @Id";

                var parameters = new { Id = paymentId };

                return await connection.QueryFirstAsync<Payment>(query, parameters);
            }
        }

        public async Task<List<Payment>> GetPaymentsByUserId(long userId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Payments WHERE userId = @UserId ";

                var parameters = new { UserId = userId };

                return (await connection.QueryAsync<Payment>(query, parameters)).ToList();
            }
        }

        public async Task UpdatePaymentStatus(long paymentId, PaymentStatus status)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"update Payments set Status = '{status}' WHERE Id = @Id";

                var parameters = new { Id = paymentId };

                await connection.ExecuteAsync(query, parameters);
            }
        }
    }
}
