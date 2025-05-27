using Backend.Api.Models;
using Nest;
using System;
using Npgsql;
using Dapper;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Diagnostics;

namespace Backend.Api.Services
{
  public class ElasticsearchService : IProductSearchService
  {
    private readonly ElasticClient _client;
    private const string IndexName = "products";
    private readonly IProductRepository? _productRepository;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly IMonitoringService? _monitoringService;

    public ElasticsearchService(string uri, IProductRepository? productRepository = null, ILogger<ElasticsearchService>? logger = null, IMonitoringService? monitoringService = null)
    {
      var settings = new ConnectionSettings(new Uri(uri))
        .DefaultIndex(IndexName)
        .ThrowExceptions(false) // Don't throw exceptions, handle them gracefully
        .RequestTimeout(TimeSpan.FromSeconds(30))
        .MaxRetryTimeout(TimeSpan.FromMinutes(2));

      _client = new ElasticClient(settings);
      _productRepository = productRepository;
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _monitoringService = monitoringService;

      // Configure retry pipeline with exponential backoff
      _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
          ShouldHandle = new PredicateBuilder().Handle<Exception>(),
          MaxRetryAttempts = 3,
          Delay = TimeSpan.FromSeconds(1),
          BackoffType = DelayBackoffType.Exponential,
          OnRetry = args =>
          {
            _logger.LogWarning("Retry {Attempt} for Elasticsearch operation after {Delay}ms. Exception: {Exception}",
              args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
            return ValueTask.CompletedTask;
          }
        })
        .Build();
    }

    public ISearchResponse<Product> SearchProducts(string? query, string? category, decimal? minPrice, decimal? maxPrice, int size = 1000)
    {
      var stopwatch = Stopwatch.StartNew();

      try
      {
        _logger.LogInformation("Searching products with query: {Query}, category: {Category}, minPrice: {MinPrice}, maxPrice: {MaxPrice}, size: {Size}",
          query, category, minPrice, maxPrice, size);

        _monitoringService?.TrackEvent("ElasticsearchSearch", new Dictionary<string, object>
        {
          { "query", query ?? "" },
          { "category", category ?? "" },
          { "minPrice", minPrice ?? 0 },
          { "maxPrice", maxPrice ?? 0 },
          { "size", size }
        });

        // Validate size parameter
        if (size <= 0 || size > 10000)
        {
          _logger.LogWarning("Invalid size parameter: {Size}. Setting to default value of 1000", size);
          size = 1000;
        }

        var searchRequest = new SearchRequest<Product>
        {
          Size = size,
          Query = new BoolQuery
          {
            Must = new List<QueryContainer>
            {
              !string.IsNullOrEmpty(query) ? new MatchQuery { Field = "name", Query = query } : null,
              !string.IsNullOrEmpty(category) ? new TermQuery { Field = "category.keyword", Value = category } : null,
              minPrice.HasValue ? new NumericRangeQuery { Field = "price", GreaterThanOrEqualTo = (double?)minPrice } : null,
              maxPrice.HasValue ? new NumericRangeQuery { Field = "price", LessThanOrEqualTo = (double?)maxPrice } : null
            }.Where(q => q != null).ToList()
          }
        };

        var response = _client.Search<Product>(searchRequest);
        stopwatch.Stop();

        if (!response.IsValid)
        {
          _monitoringService?.TrackDependency("Elasticsearch", "SearchProducts", searchRequest.ToString() ?? "", DateTime.UtcNow.Subtract(stopwatch.Elapsed), stopwatch.Elapsed, false);
          _monitoringService?.TrackException(response.OriginalException ?? new Exception(response.ServerError?.ToString() ?? "Unknown Elasticsearch error"));

          _logger.LogError("Elasticsearch search failed: {Error}", response.OriginalException?.Message ?? response.ServerError?.ToString());
          throw new InvalidOperationException($"Search failed: {response.OriginalException?.Message ?? response.ServerError?.ToString()}");
        }

        _monitoringService?.TrackDependency("Elasticsearch", "SearchProducts", searchRequest.ToString() ?? "", DateTime.UtcNow.Subtract(stopwatch.Elapsed), stopwatch.Elapsed, true);
        _monitoringService?.TrackMetric("elasticsearch_search_results", response.Total, new Dictionary<string, string>
        {
          { "query_type", !string.IsNullOrEmpty(query) ? "text_search" : "filter_only" },
          { "has_category", (!string.IsNullOrEmpty(category)).ToString() },
          { "has_price_filter", (minPrice.HasValue || maxPrice.HasValue).ToString() }
        });

        _logger.LogInformation("Search completed successfully. Found {Total} products in {Took}ms",
          response.Total, response.Took);

        return response;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        _monitoringService?.TrackException(ex, new Dictionary<string, object>
        {
          { "operation", "SearchProducts" },
          { "query", query ?? "" },
          { "category", category ?? "" }
        });

        _logger.LogError(ex, "Error occurred while searching products");
        throw;
      }
    }

    public void SyncProductsFromPostgres(string postgresConnectionString)
    {
      if (_productRepository != null)
      {
        SyncProductsFromRepository();
        return;
      }

      try
      {
        _logger.LogInformation("Starting product sync from PostgreSQL");

        using var connection = new NpgsqlConnection(postgresConnectionString);
        var products = connection.Query<Product>("SELECT id::text, name, description, price, category, brand, sku, stock, created_at FROM products").ToList();

        _logger.LogInformation("Retrieved {Count} products from PostgreSQL", products.Count);

        var successCount = 0;
        var errorCount = 0;

        foreach (var product in products)
        {
          try
          {
            var indexResponse = _retryPipeline.Execute(() => Client.IndexDocument(product));
            if (indexResponse.IsValid)
            {
              successCount++;
            }
            else
            {
              errorCount++;
              _logger.LogWarning("Failed to index product {ProductId}: {Error}",
                product.Id, indexResponse.OriginalException?.Message ?? indexResponse.ServerError?.ToString());
            }
          }
          catch (Exception ex)
          {
            errorCount++;
            _logger.LogError(ex, "Error indexing product {ProductId}", product.Id);
          }
        }

        _logger.LogInformation("Product sync completed. Success: {SuccessCount}, Errors: {ErrorCount}", successCount, errorCount);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during product sync from PostgreSQL");
        throw;
      }
    }

    public void SyncProductsFromRepository()
    {
      if (_productRepository == null)
      {
        _logger.LogError("ProductRepository is not set for sync operation");
        throw new InvalidOperationException("ProductRepository is not set.");
      }

      try
      {
        _logger.LogInformation("Starting product sync from repository");

        var products = _productRepository.GetAllProducts().ToList();

        _logger.LogInformation("Retrieved {Count} products from repository", products.Count);

        var successCount = 0;
        var errorCount = 0;

        foreach (var product in products)
        {
          try
          {
            var indexResponse = _retryPipeline.Execute(() => Client.IndexDocument(product));
            if (indexResponse.IsValid)
            {
              successCount++;
            }
            else
            {
              errorCount++;
              _logger.LogWarning("Failed to index product {ProductId}: {Error}",
                product.Id, indexResponse.OriginalException?.Message ?? indexResponse.ServerError?.ToString());
            }
          }
          catch (Exception ex)
          {
            errorCount++;
            _logger.LogError(ex, "Error indexing product {ProductId}", product.Id);
          }
        }

        _logger.LogInformation("Product sync completed. Success: {SuccessCount}, Errors: {ErrorCount}", successCount, errorCount);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during product sync from repository");
        throw;
      }
    }

    public ElasticClient Client => _client;
  }
}
