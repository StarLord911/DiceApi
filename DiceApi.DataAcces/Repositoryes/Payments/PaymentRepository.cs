using Dapper;
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
                    Status = payment.Status.ToString(),
                    UserId = payment.UserId
                };

                return await connection.ExecuteScalarAsync<long>(query, parameters);
            }
        }

        public async Task DeletePayment(long paymentId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"Delete from Payments where Id = {paymentId}";

                await connection.ExecuteAsync(query);
            }
        }

        public async Task<List<Payment>> GetAllUnConfiemedPayments()
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"SELECT * FROM Payments WHERE Status = '{PaymentStatus.New}' and FkPaymentId is not null";

                return (await connection.QueryAsync<Payment>(query)).ToList();
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

        public async Task<PaginatedList<Payment>> GetPaginatedPayments(GetPaymentsRequest request)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = new StringBuilder();

                DynamicParameters parameters = new DynamicParameters();

                if (request.StartDate != null)
                {
                    query.Append($"CreatedAt > @StartDate");
                    parameters.Add("@StartDate", request.StartDate);
                }

                if (request.EndDate != null)
                {
                    if (query.Length > 0)
                    {
                        query.Append(" AND");
                    }
                    query.Append($" CreatedAt < @EndDate");
                    parameters.Add("@EndDate", request.EndDate);
                }

                if (request.PaymentStatus != null)
                {
                    if (query.Length > 0)
                    {
                        query.Append(" AND");
                    }
                    query.Append($" Status = '{Enum.GetName(typeof(PaymentStatus), request.PaymentStatus.Value)}'");
                }

                if (!string.IsNullOrEmpty(query.ToString()))
                {
                    query.Insert(0, "where ");
                }

                parameters.Add("@Offset", (request.Pagination.PageNumber - 1) * request.Pagination.PageSize);
                parameters.Add("@PageSize", request.Pagination.PageSize);

                var countQuery = $"SELECT COUNT(*) FROM Payments {query.ToString()}";
                var paymentsQuery = $"SELECT * FROM Payments {query.ToString()} ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                int totalCount = await db.ExecuteScalarAsync<int>(countQuery, parameters);
                var payments = await db.QueryAsync<Payment>(paymentsQuery, parameters);
                var pageCount = (int)Math.Ceiling((double)totalCount / request.Pagination.PageSize);

                return new PaginatedList<Payment>(payments.ToList(), pageCount, request.Pagination.PageNumber);
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

        public async Task UpdateFkOrderId(long paymentId, long fkPaymentId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                string query = $"update Payments set FkPaymentId = '{fkPaymentId}' WHERE Id = @Id";

                var parameters = new { Id = paymentId };

                await connection.ExecuteAsync(query, parameters);
            }
        }
    }
}
