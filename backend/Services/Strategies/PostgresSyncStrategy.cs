using Backend.Api.Models;
using Nest;
using Npgsql;
using Dapper;

namespace Backend.Api.Services.Strategies
{
  public class PostgresSyncStrategy : ISyncStrategy
  {
    private readonly ElasticClient _client;
    private readonly IProductRepository? _productRepository;

    public PostgresSyncStrategy(ElasticClient client, IProductRepository? productRepository = null)
    {
      _client = client;
      _productRepository = productRepository;
    }

    public void SyncProducts(string connectionString)
    {
      if (_productRepository != null)
      {
        SyncFromRepository();
        return;
      }

      SyncFromConnection(connectionString);
    }

    private void SyncFromRepository()
    {
      var products = _productRepository!.GetAllProducts();
      foreach (var product in products)
      {
        _client.IndexDocument(product);
      }
    }

    private void SyncFromConnection(string connectionString)
    {
      using var connection = new NpgsqlConnection(connectionString);
      var products = connection.Query<Product>("SELECT id::text, name, description, price, category, brand, sku, stock, created_at FROM products").ToList();
      foreach (var product in products)
      {
        _client.IndexDocument(product);
      }
    }
  }
}
