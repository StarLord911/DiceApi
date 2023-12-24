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
    public class MinesRepository : IMinesRepository
    {
        public string _connectionString;

        public MinesRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public async Task AddMinesGame(MinesGame minesGame)
        {
            using (var IDbConnection = new SqlConnection(_connectionString))
            {
                // Создание объекта с данными для вставки
                var minesParams = new
                {
                    userId = minesGame.UserId,
                    sum = minesGame.Sum,
                    win = minesGame.Win,
                    canWin = minesGame.CanWin,
                    gameTime = DateTime.UtcNow,
                };

                // SQL-запрос на вставку данных
                string sql = @"INSERT INTO MinesGame (userId, sum, win, canWin, gameTime)
                   VALUES (@userId, @sum, @win, @canWin, @gameTime)";

                // Выполнение запроса с diceParams Dapper
                await IDbConnection.ExecuteScalarAsync(sql, minesParams);
            }
        }

        public async Task<List<MinesGame>> GetMinesGamesByUserId(long userId)
        {
            using (var IDbConnection = new SqlConnection(_connectionString))
            {
                
                string sql = @"select * from MinesGame";

                // Выполнение запроса с diceParams Dapper
                return (await IDbConnection.QueryAsync<MinesGame>(sql)).ToList();
            }
        }
    }
}
