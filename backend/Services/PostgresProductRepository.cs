using Backend.Api.Models;
using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Backend.Api.Services
{
  public class PostgresProductRepository : IProductRepository
  {
    private readonly string _connectionString;
    private readonly ILogger<PostgresProductRepository> _logger;
    private readonly ResiliencePipeline _retryPipeline;

    public PostgresProductRepository(string connectionString, ILogger<PostgresProductRepository> logger)
    {
      _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      // Configure retry pipeline for database operations
      _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
          ShouldHandle = new PredicateBuilder()
            .Handle<NpgsqlException>()
            .Handle<TimeoutException>(),
          MaxRetryAttempts = 3,
          Delay = TimeSpan.FromSeconds(1),
          BackoffType = DelayBackoffType.Exponential,
          OnRetry = args =>
          {
            _logger.LogWarning("Retry {Attempt} for database operation after {Delay}ms. Exception: {Exception}",
              args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
            return ValueTask.CompletedTask;
          }
        })
        .Build();
    }

    public IEnumerable<Product> GetAllProducts()
    {
      try
      {
        _logger.LogInformation("Retrieving all products from database");

        return _retryPipeline.Execute(() =>
        {
          using var connection = new NpgsqlConnection(_connectionString);
          connection.Open();
          var products = connection.Query<Product>(
            "SELECT id::text, name, description, price, category, brand, sku, stock, created_at FROM products"
          ).ToList();

          _logger.LogInformation("Successfully retrieved {Count} products from database", products.Count);
          return products;
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving all products from database");
        throw;
      }
    }

    public Product? GetProductById(string id)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(id))
        {
          _logger.LogWarning("GetProductById called with null or empty id");
          return null;
        }

        _logger.LogDebug("Retrieving product with id: {ProductId}", id);

        return _retryPipeline.Execute(() =>
        {
          using var connection = new NpgsqlConnection(_connectionString);
          connection.Open();
          var product = connection.QueryFirstOrDefault<Product>(
            "SELECT id::text, name, description, price, category, brand, sku, stock, created_at FROM products WHERE id = @id",
            new { id }
          );

          if (product != null)
          {
            _logger.LogDebug("Successfully retrieved product {ProductId}", id);
          }
          else
          {
            _logger.LogDebug("Product {ProductId} not found", id);
          }

          return product;
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving product {ProductId} from database", id);
        throw;
      }
    }
  }
}
