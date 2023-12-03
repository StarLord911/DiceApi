using Dapper;
using DiceApi.Common;
using DiceApi.Data.ApiReqRes;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public class CooperationRequestRepository : ICooperationRequestRepository
    {
        private readonly string _connectionString;

        public CooperationRequestRepository()
        {
            this._connectionString = DataAccesHelper.GetConnectionString(); ;
        }

        public async Task CreateCooperationRequest(CooperationRequest request)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"INSERT INTO CooperationRequests (Name, Link, Message)
                        VALUES (@Name, @Link, @Message);";

                await connection.ExecuteScalarAsync<int>(sql, request);
            }
        }

        public async Task<IEnumerable<CooperationRequest>> GetAllCooperationRequests()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM CooperationRequests";

                return await connection.QueryAsync<CooperationRequest>(sql);
            }
        }
    }
}
