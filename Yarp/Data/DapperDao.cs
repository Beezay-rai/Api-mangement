using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

namespace Yarp.Data
{
    public class DapperDao
    {
        private readonly IConfiguration _config;
        public DapperDao(IConfiguration config)
        {
            _config = config;
        }
        public async Task<T0?> ExecuteNonListAsync<T0>(string sqlQuery, object? sqlParam, CommandType queryType)
        {
            using var sqlConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
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

        public async Task<bool> ExecuteCommandAsync(string sqlQuery, object? sqlParam, CommandType queryType)
        {
            using var sqlConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            try
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);
                var result = await sqlConnection.ExecuteAsync(sqlQuery, sqlParam, commandTimeout: 120, commandType: queryType).ConfigureAwait(false);
                return result > 0;
            }
            finally
            {
                await sqlConnection.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task<IList<T0>> ExecuteQueryAsync<T0>(string sqlQuery, object? sqlParam, CommandType queryType)
        {
            using var sqlConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
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
