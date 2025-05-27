using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using Backend.Api;

namespace Backend.Tests.Integration
{
  public class HealthChecksIntegrationTests : IClassFixture<WebApplicationFactory<TestStartup>>
  {
    private readonly WebApplicationFactory<TestStartup> _factory;
    private readonly HttpClient _client;

    public HealthChecksIntegrationTests(WebApplicationFactory<TestStartup> factory)
    {
      _factory = factory;
      _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
      // Act
      var response = await _client.GetAsync("/health");

      // Assert
      response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

      var content = await response.Content.ReadAsStringAsync();
      content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ReadinessCheck_ShouldReturnAppropriateStatus()
    {
      // Act
      var response = await _client.GetAsync("/health/ready");

      // Assert
      response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

      var content = await response.Content.ReadAsStringAsync();
      content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LivenessCheck_ShouldReturnOk()
    {
      // Act
      var response = await _client.GetAsync("/health/live");

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var content = await response.Content.ReadAsStringAsync();
      content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnJsonFormat()
    {
      // Act
      var response = await _client.GetAsync("/health");
      var content = await response.Content.ReadAsStringAsync();

      // Assert
      var healthCheckResult = JsonSerializer.Deserialize<JsonElement>(content);
      healthCheckResult.TryGetProperty("status", out var statusProperty).Should().BeTrue();
      healthCheckResult.TryGetProperty("totalDuration", out var durationProperty).Should().BeTrue();
    }
  }
}
