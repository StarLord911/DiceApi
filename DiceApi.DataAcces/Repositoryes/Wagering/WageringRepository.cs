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
    public class WageringRepository : IWageringRepository
    {
        private readonly string _connectionString;

        public WageringRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public async Task AddWearing(Wager wagering)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                // Создание объекта промокода
                var promoCode = new
                {
                    UserId = wagering.UserId,
                    Wageringed = wagering.Wagering,
                    Played = wagering.Played,
                    IsActive = wagering.IsActive
                };

                // Выполнение запроса на вставку
                var query = "INSERT INTO Wagering (userId, wagering, played, isActive) " +
                            "VALUES (@UserId, @Wageringed, @Played, @IsActive)";

                await connection.ExecuteAsync(query, promoCode);
            }
        }

        public async Task DeactivateWagering(int wagerId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = "update Wagering set isActive = 0 where id = @Id";

                await connection.ExecuteAsync(query, new { Id = wagerId });
            }
        }


        public async Task ActivateWagering(int wagerId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = "update Wagering set isActive = 1 where id = @Id";

                await connection.ExecuteAsync(query, new { Id = wagerId });
            }
        }

        public async Task<Wager> GetActiveWageringByUserId(long userId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = "select * from Wagering where userId = @Id";

                return (await connection.QueryAsync<Wager>(query, new { Id = userId })).FirstOrDefault();
            }
        }

        public async Task UpdatePlayed(long userId, decimal addPlayedSub)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var played = (await GetActiveWageringByUserId(userId)).Played;

                var query = "update Wagering set played  = @Played where id = @Id";

                await connection.ExecuteAsync(query, new { Id = userId, Played = played + addPlayedSub });
            }
        }

        public async Task UpdateWagering(long userId, decimal addWagerSub)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var wager = (await GetActiveWageringByUserId(userId)).Wagering;

                var query = "update Wagering set wagering  = @Wager where id = @Id";

                await connection.ExecuteAsync(query, new { Id = userId, Wager = wager + addWagerSub });
            }
        }
    }
}
