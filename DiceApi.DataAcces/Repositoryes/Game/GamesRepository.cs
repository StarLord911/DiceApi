using Dapper;
using DiceApi.Common;
using DiceApi.Data.Data.Dice;
using DiceApi.Data.Data.Games;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes.Game
{
    public class GamesRepository : IGamesRepository
    {
        private readonly string _connectionString;


        public GamesRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }


        
        public async Task AddGame(GameModel game)
        {
            using (var IDbConnection = new SqlConnection(_connectionString))
            {
                // Создание объекта с данными для вставки
                var diceParams = new
                {
                    userId = game.UserId,
                    sum = game.BetSum,
                    win = game.Win,
                    canWin = game.CanWin,
                    gameTime = game.GameTime,
                    gameType = game.GameType

                };

                string sql = @"INSERT INTO Games (UserId, BetSum, Win, CanWin, GameTime, GameType)
                   VALUES (@userId, @sum, @win, @canWin, @gameTime, @gameType)";

                // Выполнение запроса с diceParams Dapper
                await IDbConnection.ExecuteScalarAsync(sql, diceParams);
            }
        }

        public async Task<List<GameModel>> GetGamesByUserId(long userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<GameModel>("SELECT * FROM Games where userId = @userId", new { userId })).ToList();
            }
        }
    }
}
