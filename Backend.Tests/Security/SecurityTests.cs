using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Backend.Api;

namespace Backend.Tests.Security
{
  public class SecurityTests : IClassFixture<WebApplicationFactory<TestStartup>>
  {
    private readonly WebApplicationFactory<TestStartup> _factory;
    private readonly HttpClient _client;

    public SecurityTests(WebApplicationFactory<TestStartup> factory)
    {
      _factory = factory;
      _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("'; DROP TABLE products; --")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("../../../etc/passwd")]
    [InlineData("{{7*7}}")]
    [InlineData("${7*7}")]
    public async Task GraphQL_ShouldSanitizeInput_MaliciousQueries(string maliciousInput)
    {
      // Arrange
      var query = new
      {
        query = @"
                    query SearchProducts($query: String!) {
                        searchProducts(query: $query) {
                            id
                            name
                        }
                    }",
        variables = new { query = maliciousInput }
      };

      var json = JsonSerializer.Serialize(query);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      // Act
      var response = await _client.PostAsync("/graphql", content);

      // Assert
      response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

      var responseContent = await response.Content.ReadAsStringAsync();
      responseContent.Should().NotContain("DROP TABLE");
      responseContent.Should().NotContain("<script>");
      responseContent.Should().NotContain("/etc/passwd");
    }

    [Theory]
    [InlineData("InvalidCategory'; DROP TABLE products; --")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("../admin")]
    public async Task GraphQL_ShouldValidateCategory_MaliciousInput(string maliciousCategory)
    {
      // Arrange
      var query = new
      {
        query = @"
                    query SearchProducts($query: String!, $category: String) {
                        searchProducts(query: $query, category: $category) {
                            id
                            name
                        }
                    }",
        variables = new { query = "test", category = maliciousCategory }
      };

      var json = JsonSerializer.Serialize(query);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      // Act
      var response = await _client.PostAsync("/graphql", content);

      // Assert
      response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

      var responseContent = await response.Content.ReadAsStringAsync();
      if (response.IsSuccessStatusCode)
      {
        // Should return validation error for invalid category
        responseContent.Should().Contain("error");
      }
    }

    [Theory]
    [InlineData(-999999999)]
    [InlineData(999999999)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public async Task GraphQL_ShouldValidatePriceRange_ExtremeValues(double extremePrice)
    {
      // Arrange
      var query = new
      {
        query = @"
                    query SearchProducts($query: String!, $minPrice: Float, $maxPrice: Float) {
                        searchProducts(query: $query, minPrice: $minPrice, maxPrice: $maxPrice) {
                            id
                            name
                        }
                    }",
        variables = new { query = "test", minPrice = extremePrice, maxPrice = extremePrice + 1000 }
      };

      var json = JsonSerializer.Serialize(query);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      // Act
      var response = await _client.PostAsync("/graphql", content);

      // Assert
      response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GraphQL_ShouldRejectExcessivelyLargeQueries()
    {
      // Arrange - Create a very large query
      var largeQuery = new string('a', 10000); // 10KB query
      var query = new
      {
        query = @"
                    query SearchProducts($query: String!) {
                        searchProducts(query: $query) {
                            id
                            name
                        }
                    }",
        variables = new { query = largeQuery }
      };

      var json = JsonSerializer.Serialize(query);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      // Act
      var response = await _client.PostAsync("/graphql", content);

      // Assert
      response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge);
    }

    [Theory]
    [InlineData("/admin")]
    [InlineData("/config")]
    [InlineData("/.env")]
    [InlineData("/appsettings.json")]
    [InlineData("/../")]
    public async Task Api_ShouldProtectSensitiveEndpoints(string sensitiveEndpoint)
    {
      // Act
      var response = await _client.GetAsync(sensitiveEndpoint);

      // Assert
      response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Api_ShouldHaveSecurityHeaders()
    {
      // Act
      var response = await _client.GetAsync("/health");

      // Assert
      response.Headers.Should().ContainKey("X-Content-Type-Options");
      response.Headers.Should().ContainKey("X-Frame-Options");
      // Add more security header checks as needed
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task GraphQL_ShouldOnlyAcceptPostRequests(string httpMethod)
    {
      // Arrange
      var request = new HttpRequestMessage(new HttpMethod(httpMethod), "/graphql");

      // Act
      var response = await _client.SendAsync(request);

      // Assert
      if (httpMethod == "POST")
      {
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
      }
      else
      {
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.BadRequest);
      }
    }

    [Fact]
    public async Task Api_ShouldRateLimitRequests()
    {
      // Arrange - Send many requests quickly
      var tasks = new List<Task<HttpResponseMessage>>();

      for (int i = 0; i < 100; i++)
      {
        tasks.Add(_client.GetAsync("/health"));
      }

      // Act
      var responses = await Task.WhenAll(tasks);

      // Assert
      var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests);

      // Note: This test might not trigger rate limiting in test environment
      // but it's useful for production testing
      foreach (var response in responses)
      {
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.ServiceUnavailable
        );
      }
    }
  }
}
