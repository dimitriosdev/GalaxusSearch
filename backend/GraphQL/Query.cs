using Backend.Api.DTOs;
using Backend.Api.Models;
using Backend.Api.Services;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Api.GraphQL
{
  public class Query
  {
    public IEnumerable<ProductDto> SearchProducts(
        [Service] IProductSearchService productSearchService,
        string? query,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        int size = 1000)
    {
      var result = productSearchService.SearchProducts(query, category, minPrice, maxPrice, size);
      return result.Documents.Select(ProductMapper.ToDto);
    }
  }
}
