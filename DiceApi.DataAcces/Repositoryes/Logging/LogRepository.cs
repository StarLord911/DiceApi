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

        public async Task LogException(Exception exception)
        {
            string message = $"Exception: {exception.Message}\nStackTrace: {exception.StackTrace.Substring(0, 200)}";
            await AddLog(message, "Exception");
        }

        private async Task AddLog(string message, string level)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Logs (Timestamp, Level, Message) VALUES (@Timestamp, @Level, @Message)";
                await connection.ExecuteAsync(query, new { Timestamp = DateTime.UtcNow, Level = level, Message = message });
            }
        }

    }
}
