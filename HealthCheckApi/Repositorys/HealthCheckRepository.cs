using Dapper;
using HealthCheckerApi.Contacts;
using HealthCheckerApi.Data;
using HealthCheckerApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.InteropServices;

namespace HealthCheckerApi.Repositorys
{
    public class HealthCheckRepository : IHealthCheckRepository
    {
        private readonly DapperContext _dapperContext;

        public HealthCheckRepository(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<List<HealthCheckStatus>> GetLatestHealthStatusAsync()
        {
            var dataList = new List<HealthCheckStatus>();

            using (var db = _dapperContext.CreateHealthDbConnection())
            {
                string query = "SELECT * FROM HealthCheckLogs ORDER BY CheckedAt DESC";

                var datas= await db.QueryAsync<HealthCheckStatus>(query);
                dataList= datas.ToList();
            }

            return dataList;
        }
    }
}
