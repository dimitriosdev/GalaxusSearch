using Backend.Api.Models;
using Nest;

namespace Backend.Api.Services.Strategies
{
  public class ElasticsearchSearchStrategy : ISearchStrategy
  {
    private readonly ElasticClient _client;

    public ElasticsearchSearchStrategy(ElasticClient client)
    {
      _client = client;
    }

    public ISearchResponse<Product> Search(string? query, string? category, decimal? minPrice, decimal? maxPrice, int size = 1000)
    {
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
      return _client.Search<Product>(searchRequest);
    }
  }
}
