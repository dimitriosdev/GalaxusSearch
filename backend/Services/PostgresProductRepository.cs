using Backend.Api.Models;
using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Api.Services
{
  public class PostgresProductRepository : IProductRepository
  {
    private readonly string _connectionString;
    public PostgresProductRepository(string connectionString)
    {
      _connectionString = connectionString;
    }

    public IEnumerable<Product> GetAllProducts()
    {
      using var connection = new NpgsqlConnection(_connectionString);
      return connection.Query<Product>("SELECT id::text, name, description, price, category, brand, sku, stock, created_at FROM products").ToList();
    }

    public Product? GetProductById(string id)
    {
      using var connection = new NpgsqlConnection(_connectionString);
      return connection.QueryFirstOrDefault<Product>("SELECT id::text, name, description, price, category, brand, sku, stock, created_at FROM products WHERE id = @id", new { id });
    }
  }
}
