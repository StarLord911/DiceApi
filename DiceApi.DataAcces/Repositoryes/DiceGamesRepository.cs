using Dapper;
using DiceApi.Common;
using DiceApi.Data.Data.Dice;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public class DiceGamesRepository : IDiceGamesRepository
    {
        private readonly string _connectionString;


        public DiceGamesRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }


        public async Task Add(DiceGame diceGame)
        {
            using (var IDbConnection = new SqlConnection(_connectionString))
            {
                // Создание объекта с данными для вставки
                var diceParams = new
                {
                    userId = diceGame.UserId,
                    persent = diceGame.Persent,
                    sum = diceGame.Sum,
                    win = diceGame.Win,
                    canWin = diceGame.CanWin,
                    gameTime = diceGame.GameTime
                };

                // SQL-запрос на вставку данных
                string sql = @"INSERT INTO DiceGame (userId, persent, sum, win, canWin, gameTime)
                   VALUES (@userId, @persent, @sum, @win, @canWin, @gameTime)";

                // Выполнение запроса с diceParams Dapper
                await IDbConnection.ExecuteScalarAsync(sql, diceParams);
            }
        }

        public async Task<List<DiceGame>> GetAll()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<DiceGame>("SELECT * FROM DiceGame")).ToList();
            }
        }

        public async Task<List<DiceGame>> GetByUserId(long userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<DiceGame>("SELECT * FROM DiceGame where userId = @userId", new { userId })).ToList();
            }
        }

        public async Task<List<DiceGame>> GetLastGames()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<DiceGame>("SELECT top(10) * FROM DiceGame order by gameTime desc")).ToList();
            }
        }
    }
}
