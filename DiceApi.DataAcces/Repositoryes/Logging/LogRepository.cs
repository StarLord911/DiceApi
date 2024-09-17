using Dapper;
using DiceApi.Common;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public class LogRepository : ILogRepository
    {
        private readonly string _connectionString;

        public LogRepository()
        {
            _connectionString = DataAccesHelper.GetConnectionString();
        }

        public async Task LogError(string message)
        {
            await AddLog(message, "Error");
        }

        public async Task LogInfo(string message)
        {
            await AddLog(message, "Info");
        }

        public async Task LogException(string message, Exception exception)
        {
            string trace = exception.StackTrace;
            if (exception.StackTrace.Count() >= 500)
            {
                trace = exception.StackTrace.Substring(1, 490);
            }

            string log = $"{message} Exception: {exception.Message}\nStackTrace: {trace}";
            await AddLog(log, "Exception");
        }

        private async Task AddLog(string message, string level)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Logs (Timestamp, Level, Message) VALUES (@Timestamp, @Level, @Message)";
                await connection.ExecuteAsync(query, new { Timestamp = DateTime.UtcNow.GetMSKDateTime(), Level = level, Message = message });
            }
        }

        public async Task LogGame(string message)
        {
            await AddLog(message, "Game");
        }

        public async Task LogInfo(string message, string level)
        {
            await AddLog(message, level);
        }
    }
}
