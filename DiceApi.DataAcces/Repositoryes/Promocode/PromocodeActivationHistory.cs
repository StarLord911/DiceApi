using Dapper;
using DiceApi.Common;
using DiceApi.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public class PromocodeActivationHistory : IPromocodeActivationHistory
    {
        private readonly string _connectionString;

        public PromocodeActivationHistory()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
            //PromoCodeActivationHistory
        }

        public async Task AddPromocodeActivation(PrimocodeActivation activation)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                // Создание объекта промокода
                var promoCode = new
                {
                    Promocode = activation.Promocode,
                    UserId = activation.UserId
                };

                // Выполнение запроса на вставку
                var query = $@"INSERT INTO PromoCodeActivationHistory (userId, promocode)
                            VALUES (@UserId, @Promocode)";

                await connection.ExecuteAsync(query, promoCode);
            }
        }

        public async Task<List<PrimocodeActivation>> GetPromocodeActivates(string promocode)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = "select * from PromoCodeActivationHistory where promocode = @code";

                return (await connection.QueryAsync<PrimocodeActivation>(query, new { code = promocode })).ToList();
            }
        }

        public async Task<List<PrimocodeActivation>> GetPromocodeActivatesByUserId(long userId)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var query = "select * from PromoCodeActivationHistory where userId = @id";

                return (await connection.QueryAsync<PrimocodeActivation>(query, new { id = userId })).ToList();
            }
        }
    }
}