using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace OnePoint.PDK.Data
{
    public sealed class OnePointDao
    {
        private readonly string _connectionString;
        public OnePointDao(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<T0?> ExecuteNonListAsync<T0>(string sqlQuery, object? sqlParam, CommandType queryType)
        {
            using var sqlConnection = new SqlConnection(_connectionString);
            try
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);
                var result = await sqlConnection.QuerySingleOrDefaultAsync<T0>(sqlQuery, sqlParam, commandTimeout: 120, commandType: queryType).ConfigureAwait(false);
                return result;
            }
            finally
            {
                await sqlConnection.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> ExecuteCommandAsync(string sqlQuery, object? sqlParam, CommandType queryType)
        {
            using var sqlConnection = new SqlConnection(_connectionString);
            try
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);
                var result = await sqlConnection.ExecuteAsync(sqlQuery, sqlParam, commandTimeout: 120, commandType: queryType).ConfigureAwait(false);
                return result;
            }
            finally
            {
                await sqlConnection.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<T0>> ExecuteListAsync<T0>(string sqlQuery, object? sqlParam, CommandType queryType)
        {
            using var sqlConnection = new SqlConnection(_connectionString);
            try
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);
                var result = await sqlConnection.QueryAsync<T0>(sqlQuery, sqlParam, commandTimeout: 120, commandType: queryType).ConfigureAwait(false);
                return result.ToList();
            }
            finally
            {
                await sqlConnection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
