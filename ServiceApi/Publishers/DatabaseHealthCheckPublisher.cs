using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceApi.Data;
using ServiceApi.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

public class DatabaseHealthCheckPublisher : IHealthCheckPublisher
{
    private readonly DapperContext _dapperContext;

    public DatabaseHealthCheckPublisher(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        const string insertQuery = @"INSERT INTO HealthCheckLogs (CheckName, Status, Description, DurationMs, CheckedAt)
            VALUES (@CheckName, @Status, @Description, @DurationMs, @CheckedAt)";

        // Use IDbConnection instead of manually opening SqlConnection
        using (var connection = _dapperContext.CreateHealthDbConnection())
        {
            var healthCheckLog = new HealthCheckStatus();
            foreach (var entry in report.Entries)
            {
                healthCheckLog = new HealthCheckStatus();

                healthCheckLog.CheckName= entry.Key;
                healthCheckLog.Status = entry.Value.Status.ToString();
                healthCheckLog.Description = entry.Value.Description ?? entry.Value.Exception?.Message;
                healthCheckLog.DurationMs= entry.Value.Duration.TotalMilliseconds;
                healthCheckLog.CheckedAt= DateTime.UtcNow;


                // Dapper will automatically open and close the connection as needed
                await connection.ExecuteAsync(insertQuery, healthCheckLog);
            }
        }
    }


}