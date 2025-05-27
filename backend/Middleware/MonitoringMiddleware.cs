using System.Diagnostics;
using System.Text.Json;

namespace Backend.Api.Middleware
{
  public class MonitoringMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<MonitoringMiddleware> _logger;
    private readonly Services.IMonitoringService _monitoringService;

    public MonitoringMiddleware(RequestDelegate next, ILogger<MonitoringMiddleware> logger, Services.IMonitoringService monitoringService)
    {
      _next = next ?? throw new ArgumentNullException(nameof(next));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var startTime = DateTime.UtcNow;
      var stopwatch = Stopwatch.StartNew();

      try
      {
        // Track request start
        var requestInfo = new
        {
          Method = context.Request.Method,
          Path = context.Request.Path,
          QueryString = context.Request.QueryString.ToString(),
          UserAgent = context.Request.Headers["User-Agent"].ToString(),
          RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
          StartTime = startTime
        };

        _logger.LogInformation("Request started: {RequestInfo}", JsonSerializer.Serialize(requestInfo));

        await _next(context);

        stopwatch.Stop();

        // Track successful request
        _monitoringService.TrackRequest(
            $"{context.Request.Method} {context.Request.Path}",
            startTime,
            stopwatch.Elapsed,
            context.Response.StatusCode.ToString(),
            context.Response.StatusCode < 400
        );

        // Track performance metrics
        _monitoringService.TrackMetric("request_duration_ms", stopwatch.Elapsed.TotalMilliseconds, new Dictionary<string, string>
                {
                    { "method", context.Request.Method },
                    { "path", context.Request.Path },
                    { "status_code", context.Response.StatusCode.ToString() }
                });

        _logger.LogInformation("Request completed: {Method} {Path} - {StatusCode} in {Duration}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.Elapsed.TotalMilliseconds);
      }
      catch (Exception ex)
      {
        stopwatch.Stop();

        // Track failed request
        _monitoringService.TrackRequest(
            $"{context.Request.Method} {context.Request.Path}",
            startTime,
            stopwatch.Elapsed,
            "500",
            false
        );

        // Track exception
        _monitoringService.TrackException(ex, new Dictionary<string, object>
                {
                    { "method", context.Request.Method },
                    { "path", context.Request.Path.ToString() },
                    { "query", context.Request.QueryString.ToString() }
                });

        _logger.LogError(ex, "Request failed: {Method} {Path} in {Duration}ms",
            context.Request.Method,
            context.Request.Path,
            stopwatch.Elapsed.TotalMilliseconds);

        // Re-throw to let other middleware handle the error
        throw;
      }
    }
  }
}
