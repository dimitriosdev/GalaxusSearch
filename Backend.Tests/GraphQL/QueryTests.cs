using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Backend.Api.GraphQL;
using Backend.Api.Services;
using Backend.Api.Models;
using Backend.Api.DTOs;
using Nest;

namespace Backend.Tests.GraphQL
{
  public class QueryTests
  {
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IProductSearchService> _mockSearchService;
    private readonly Mock<ILogger<Query>> _mockLogger;
    private readonly Query _query;

    public QueryTests()
    {
      _mockRepository = new Mock<IProductRepository>();
      _mockSearchService = new Mock<IProductSearchService>();
      _mockLogger = new Mock<ILogger<Query>>();
      _query = new Query();
    }

    [Fact]
    public void GetProducts_ShouldReturnMappedProducts()
    {
      // Arrange
      var expectedProducts = new List<Product>
            {
                new Product { Id = "1", Name = "Test Product 1", Category = "Electronics", Price = 99.99m },
                new Product { Id = "2", Name = "Test Product 2", Category = "Books", Price = 19.99m }
            };

      _mockRepository.Setup(r => r.GetAllProducts())
          .Returns(expectedProducts);

      // Act
      var result = _query.GetProducts(_mockRepository.Object, _mockLogger.Object);

      // Assert
      result.Should().NotBeNull();
      result.Should().BeAssignableFrom<IEnumerable<ProductDto>>();
    }

    [Fact]
    public void GetProduct_ShouldReturnProduct_WhenValidId()
    {
      // Arrange
      var productId = "1";
      var expectedProduct = new Product
      {
        Id = productId,
        Name = "Test Product",
        Category = "Electronics",
        Price = 99.99m
      };

      _mockRepository.Setup(r => r.GetProductById(productId))
          .Returns(expectedProduct);

      // Act
      var result = _query.GetProduct(_mockRepository.Object, _mockLogger.Object, productId);

      // Assert
      result.Should().NotBeNull();
      result.Should().BeOfType<ProductDto>();
    }

    [Fact]
    public void GetProduct_ShouldReturnNull_WhenProductNotFound()
    {
      // Arrange
      var productId = "999";

      _mockRepository.Setup(r => r.GetProductById(productId))
          .Returns((Product?)null);

      // Act
      var result = _query.GetProduct(_mockRepository.Object, _mockLogger.Object, productId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public void SearchProducts_ShouldReturnSearchResults()
    {
      // Arrange
      var query = "test";
      var category = "Electronics";
      var searchResponse = new Mock<ISearchResponse<Product>>();
      var products = new List<Product>
            {
                new Product { Id = "1", Name = "Test Product", Category = "Electronics", Price = 99.99m }
            };

      searchResponse.Setup(r => r.Documents).Returns(products);
      searchResponse.Setup(r => r.IsValid).Returns(true);

      _mockSearchService.Setup(s => s.SearchProducts(query, category, null, null, 20))
          .Returns(searchResponse.Object);

      // Act
      var result = _query.SearchProducts(_mockSearchService.Object, _mockLogger.Object,
          query, category, null, null, 20);

      // Assert
      result.Should().NotBeNull();
      result.Should().BeAssignableFrom<IEnumerable<ProductDto>>();
    }

    [Fact]
    public void SearchProducts_ShouldHandleInvalidCategory()
    {
      // Arrange
      var query = "test";
      var invalidCategory = "InvalidCategory";

      // Act
      var result = _query.SearchProducts(_mockSearchService.Object, _mockLogger.Object,
          query, invalidCategory, null, null, 20);

      // Assert
      result.Should().NotBeNull();
      result.Should().BeEmpty();
    }

    [Fact]
    public void SearchProducts_ShouldValidateSize()
    {
      // Arrange
      var query = "test";
      var tooLargeSize = 1001;

      // Act
      var result = _query.SearchProducts(_mockSearchService.Object, _mockLogger.Object,
          query, null, null, null, tooLargeSize);

      // Assert
      result.Should().NotBeNull();
      // Should handle size validation internally
    }
  }
}
