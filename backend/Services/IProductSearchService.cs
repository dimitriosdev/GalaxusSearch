using Backend.Api.Models;
using Nest;
using System.Collections.Generic;

namespace Backend.Api.Services
{
  public interface IProductSearchService
  {
    ISearchResponse<Product> SearchProducts(string? query, string? category, decimal? minPrice, decimal? maxPrice, int size = 1000);
    void SyncProductsFromPostgres(string postgresConnectionString);
    ElasticClient Client { get; }
  }
}
