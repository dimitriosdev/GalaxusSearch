using Backend.Api.Models;
using Nest;
using System;
using Npgsql;
using Dapper;

namespace Backend.Api.Services
{
  public class ElasticsearchService : IProductSearchService
  {
    private readonly ElasticClient _client;
    private const string IndexName = "products";
    private readonly IProductRepository? _productRepository;

    public ElasticsearchService(string uri, IProductRepository? productRepository = null)
    {
      var settings = new ConnectionSettings(new Uri(uri)).DefaultIndex(IndexName);
      _client = new ElasticClient(settings);
      _productRepository = productRepository;
    }

    public ISearchResponse<Product> SearchProducts(string? query, string? category, decimal? minPrice, decimal? maxPrice, int size = 1000)
    {
      var searchRequest = new SearchRequest<Product>
      {
        Size = size, // Allow up to 1000 results by default
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
      return _client.Search<Product>(searchRequest);
    }

    public void SyncProductsFromPostgres(string postgresConnectionString)
    {
      if (_productRepository != null)
      {
        SyncProductsFromRepository();
        return;
      }
      using var connection = new NpgsqlConnection(postgresConnectionString);
      var products = connection.Query<Product>("SELECT id::text, name, description, price, category, brand, sku, stock, created_at FROM products").ToList();
      foreach (var product in products)
      {
        Client.IndexDocument(product);
      }
    }
    public void SyncProductsFromRepository()
    {
      if (_productRepository == null) throw new InvalidOperationException("ProductRepository is not set.");
      var products = _productRepository.GetAllProducts();
      foreach (var product in products)
      {
        Client.IndexDocument(product);
      }
    }

    public ElasticClient Client => _client;
  }
}
