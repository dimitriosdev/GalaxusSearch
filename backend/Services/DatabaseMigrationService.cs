using Npgsql;
using Microsoft.Extensions.Logging;
using SystemPath = System.IO.Path;

namespace Backend.Api.Services
{
  public interface IDatabaseMigrationService
  {
    Task MigrateAsync();
    Task SeedAsync();
  }

  public class DatabaseMigrationService : IDatabaseMigrationService
  {
    private readonly string _connectionString;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(string connectionString, ILogger<DatabaseMigrationService> logger)
    {
      _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task MigrateAsync()
    {
      try
      {
        _logger.LogInformation("Starting database migration");

        var migrationFiles = Directory.GetFiles(
            SystemPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Migrations"),
            "*.sql"
        ).OrderBy(f => f);

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var migrationFile in migrationFiles)
        {
          _logger.LogInformation("Executing migration: {MigrationFile}", SystemPath.GetFileName(migrationFile));

          var sql = await File.ReadAllTextAsync(migrationFile);
          using var command = new NpgsqlCommand(sql, connection);
          await command.ExecuteNonQueryAsync();

          _logger.LogInformation("Migration completed: {MigrationFile}", SystemPath.GetFileName(migrationFile));
        }

        _logger.LogInformation("Database migration completed successfully");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during database migration");
        throw;
      }
    }

    public async Task SeedAsync()
    {
      try
      {
        _logger.LogInformation("Starting database seeding");

        var seedFiles = Directory.GetFiles(
            SystemPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Seeds"),
            "*.sql"
        ).OrderBy(f => f);

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var seedFile in seedFiles)
        {
          _logger.LogInformation("Executing seed: {SeedFile}", SystemPath.GetFileName(seedFile));

          var sql = await File.ReadAllTextAsync(seedFile);
          using var command = new NpgsqlCommand(sql, connection);
          await command.ExecuteNonQueryAsync();

          _logger.LogInformation("Seed completed: {SeedFile}", SystemPath.GetFileName(seedFile));
        }

        _logger.LogInformation("Database seeding completed successfully");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during database seeding");
        throw;
      }
    }
  }
}
