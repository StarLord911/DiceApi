using DiceApi.Common;
using DiceApi.Data;
using Microsoft.Data.SqlClient;
using System;
using Dapper;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;
using DiceApi.Data.ApiModels;

namespace DiceApi.DataAcces.Repositoryes
{
    public class PromocodeRepository : IPromocodeRepository
    {
        private readonly string _connectionString;

        public PromocodeRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public async Task CreatePromocode(Promocode promocode)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                // Создание объекта промокода
                var promoCode = new
                {
                    Promocode = promocode.PromoCode,
                    ActivationCount = promocode.ActivationCount,
                    IsActive = promocode.IsActive,
                    BallanceAdd = promocode.BallanceAdd,
                    Wagering = promocode.Wagering
                };

                // Выполнение запроса на вставку
                var query = "INSERT INTO PromoCodes (promocode, activationCount, isActive, ballanceAdd, wagering) " +
                            "VALUES (@Promocode, @ActivationCount, @IsActive, @BallanceAdd, @Wagering)";

                 await connection.ExecuteAsync(query, promoCode);
            }
        }

        public async Task DiactivatePromocode(string promocode)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.QueryFirstAsync<Promocode>("update PromoCodes set isActive = 0 WHERE promocode = @code", new { code = promocode });
            }
        }

        public async Task<List<Promocode>> GetAllPromocodes()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<Promocode>("SELECT * FROM PromoCodes")).ToList();
            }
        }

        public async Task<Promocode> GetPromocode(string promocode)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<Promocode>("SELECT * FROM PromoCodes WHERE promocode = @code", new { code = promocode })).FirstOrDefault();
            }
        }

        public async Task<List<Promocode>> GetPromocodeByLike(string name)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<Promocode>($"SELECT * FROM PromoCodes where promocode like '%{name}%'")).ToList();
            }
        }

        public async Task<bool> IsPromocodeContains(string promocode)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<Promocode>("SELECT * FROM PromoCodes WHERE promocode = @code", new { code = promocode })).Any();
            }
        }
    }
}
