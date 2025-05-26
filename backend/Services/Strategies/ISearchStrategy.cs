using Backend.Api.Models;
using Nest;

namespace Backend.Api.Services.Strategies
{
  public interface ISearchStrategy
  {
    ISearchResponse<Product> Search(string? query, string? category, decimal? minPrice, decimal? maxPrice, int size = 1000);
  }
}
