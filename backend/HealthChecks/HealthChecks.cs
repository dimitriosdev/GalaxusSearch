using Microsoft.Extensions.Diagnostics.HealthChecks;
using Backend.Api.Services;
using Npgsql;
using Nest;

namespace Backend.Api.HealthChecks
{
  public class PostgresHealthCheck : IHealthCheck
  {
    private readonly string _connectionString;

    public PostgresHealthCheck(string connectionString)
    {
      _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
      try
      {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(cancellationToken);

        return HealthCheckResult.Healthy("PostgreSQL database is healthy");
      }
      catch (Exception ex)
      {
        return HealthCheckResult.Unhealthy("PostgreSQL database is unhealthy", ex);
      }
    }
  }

  public class ElasticsearchHealthCheck : IHealthCheck
  {
    private readonly ElasticClient _client;

    public ElasticsearchHealthCheck(ElasticClient client)
    {
      _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
      try
      {
        var response = await _client.PingAsync(ct: cancellationToken);

        if (response.IsValid)
        {
          return HealthCheckResult.Healthy("Elasticsearch is healthy");
        }
        else
        {
          return HealthCheckResult.Unhealthy($"Elasticsearch is unhealthy: {response.OriginalException?.Message}");
        }
      }
      catch (Exception ex)
      {
        return HealthCheckResult.Unhealthy("Elasticsearch is unhealthy", ex);
      }
    }
  }
}
