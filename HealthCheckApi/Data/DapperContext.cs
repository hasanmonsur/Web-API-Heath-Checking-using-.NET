using Microsoft.Data.SqlClient;
using System.Data;

namespace HealthCheckerApi.Data
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        //private readonly string _connectionString;
        private readonly string _connectionHealthString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
           // _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connectionHealthString = _configuration.GetConnectionString("HealthDbConnection");
        }

        //public IDbConnection CreateConnection()
        //{
        //    return new SqlConnection(_connectionString);
        //}

        public IDbConnection CreateHealthDbConnection()
        {
            return new SqlConnection(_connectionHealthString);
        }
    }
}
