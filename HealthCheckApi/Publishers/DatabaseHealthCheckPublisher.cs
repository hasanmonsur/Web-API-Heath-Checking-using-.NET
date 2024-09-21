using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceApi.Data;
using System;
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
        using (var connection = _dapperContext.CreateHealthDbConnection())
            {
            object value = await connection.OpenAsync(cancellationToken);

                foreach (var entry in report.Entries)
                {
                    var name = entry.Key;
                    var status = entry.Value.Status.ToString();
                    var description = entry.Value.Description;
                    var duration = entry.Value.Duration.TotalMilliseconds;

                    // Log each health check result to the database
                    var command = new SqlCommand(
                        "INSERT INTO HealthCheckLogs (CheckName, Status, Description, DurationMs, CheckedAt) " +
                        "VALUES (@name, @status, @description, @duration, @checkedAt)", connection);

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@duration", duration);
                    command.Parameters.AddWithValue("@checkedAt", DateTime.UtcNow);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }
    }


