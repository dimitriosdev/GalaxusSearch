using Backend.Api.DTOs;
using Backend.Api.Models;
using Backend.Api.Services;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Backend.Api.GraphQL
{
  public class Query
  {
    private static readonly string[] ValidCategories =
    {
      "Electronics", "Fashion", "Home & Kitchen", "Gaming", "Automotive", "Sports", "Books", "Music"
    };

    private static readonly Regex QuerySanitizationRegex = new(@"[<>""'&]", RegexOptions.Compiled);

    public IEnumerable<ProductDto> SearchProducts(
        [Service] IProductSearchService productSearchService,
        [Service] ILogger<Query> logger,
        string? query,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        int size = 20)
    {
      // Comprehensive input validation
      ValidateSearchParameters(query, category, minPrice, maxPrice, size);

      try
      {
        // Sanitize query input
        var sanitizedQuery = SanitizeQuery(query);

        logger.LogInformation("Executing product search with parameters: Query={Query}, Category={Category}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, Size={Size}",
          sanitizedQuery, category, minPrice, maxPrice, size);

        var result = productSearchService.SearchProducts(sanitizedQuery, category, minPrice, maxPrice, size);
        var products = result.Documents.Select(ProductMapper.ToDto).ToList();

        logger.LogInformation("Search completed successfully. Found {Count} products", products.Count);

        return products;
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to search products");
        throw new InvalidOperationException("Failed to search products", ex);
      }
    }

    public ProductDto? GetProduct(
        [Service] IProductRepository productRepository,
        [Service] ILogger<Query> logger,
        string id)
    {
      // Validate product ID
      if (string.IsNullOrWhiteSpace(id))
        throw new ArgumentException("Product ID cannot be null or empty", nameof(id));

      if (id.Length > 50)
        throw new ArgumentException("Product ID cannot exceed 50 characters", nameof(id));

      // Basic sanitization for ID
      if (QuerySanitizationRegex.IsMatch(id))
        throw new ArgumentException("Product ID contains invalid characters", nameof(id));

      try
      {
        logger.LogDebug("Retrieving product with ID: {ProductId}", id);

        var product = productRepository.GetProductById(id);

        if (product == null)
        {
          logger.LogDebug("Product not found: {ProductId}", id);
          return null;
        }

        logger.LogDebug("Product retrieved successfully: {ProductId}", id);
        return ProductMapper.ToDto(product);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to retrieve product {ProductId}", id);
        throw new InvalidOperationException($"Failed to retrieve product {id}", ex);
      }
    }

    private static void ValidateSearchParameters(string? query, string? category, decimal? minPrice, decimal? maxPrice, int size)
    {
      // Size validation
      if (size <= 0 || size > 1000)
        throw new ArgumentException("Size must be between 1 and 1000", nameof(size));

      // Price validation
      if (minPrice.HasValue && minPrice < 0)
        throw new ArgumentException("Minimum price cannot be negative", nameof(minPrice));

      if (maxPrice.HasValue && maxPrice < 0)
        throw new ArgumentException("Maximum price cannot be negative", nameof(maxPrice));

      if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
        throw new ArgumentException("Minimum price cannot be greater than maximum price");

      // Query validation
      if (!string.IsNullOrEmpty(query))
      {
        if (query.Length > 100)
          throw new ArgumentException("Query cannot exceed 100 characters", nameof(query));

        if (string.IsNullOrWhiteSpace(query.Trim()))
          throw new ArgumentException("Query cannot be only whitespace", nameof(query));
      }

      // Category validation
      if (!string.IsNullOrEmpty(category) && !ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
      {
        throw new ArgumentException($"Invalid category. Valid categories are: {string.Join(", ", ValidCategories)}", nameof(category));
      }

      // Price range validation (business rule)
      if (maxPrice.HasValue && maxPrice > 100000)
        throw new ArgumentException("Maximum price cannot exceed $100,000", nameof(maxPrice));
    }

    private static string? SanitizeQuery(string? query)
    {
      if (string.IsNullOrEmpty(query))
        return query;

      // Remove potentially dangerous characters
      var sanitized = QuerySanitizationRegex.Replace(query, "");

      // Trim whitespace
      sanitized = sanitized.Trim();

      return string.IsNullOrEmpty(sanitized) ? null : sanitized;
    }
  }
}
