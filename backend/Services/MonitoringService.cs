using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Backend.Api.Services
{
  public interface IMonitoringService
  {
    void TrackEvent(string eventName, Dictionary<string, object>? properties = null);
    void TrackException(Exception exception, Dictionary<string, object>? properties = null);
    void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null);
    void TrackDependency(string dependencyType, string dependencyName, string data, DateTime startTime, TimeSpan duration, bool success);
    void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success);
    IDisposable StartOperation(string operationName);
  }

  public class MonitoringService : IMonitoringService
  {
    private readonly ILogger<MonitoringService> _logger;
    private readonly string _applicationName;
    private readonly string _environment;

    public MonitoringService(ILogger<MonitoringService> logger, IConfiguration configuration)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _applicationName = configuration["ApplicationName"] ?? "GalaxusProductSearch";
      _environment = configuration["Environment"] ?? "Development";
    }

    public void TrackEvent(string eventName, Dictionary<string, object>? properties = null)
    {
      var eventData = new
      {
        EventName = eventName,
        Timestamp = DateTime.UtcNow,
        Application = _applicationName,
        Environment = _environment,
        Properties = properties ?? new Dictionary<string, object>()
      };

      _logger.LogInformation("Event: {EventData}", JsonSerializer.Serialize(eventData));

      // TODO: Send to external monitoring service (Application Insights, DataDog, etc.)
      // await SendToExternalMonitoringService(eventData);
    }

    public void TrackException(Exception exception, Dictionary<string, object>? properties = null)
    {
      var exceptionData = new
      {
        Exception = new
        {
          Type = exception.GetType().Name,
          Message = exception.Message,
          StackTrace = exception.StackTrace,
          InnerException = exception.InnerException?.Message
        },
        Timestamp = DateTime.UtcNow,
        Application = _applicationName,
        Environment = _environment,
        Properties = properties ?? new Dictionary<string, object>()
      };

      _logger.LogError(exception, "Exception: {ExceptionData}", JsonSerializer.Serialize(exceptionData));

      // TODO: Send to external monitoring service
      // await SendToExternalMonitoringService(exceptionData);
    }

    public void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
      var metricData = new
      {
        MetricName = metricName,
        Value = value,
        Timestamp = DateTime.UtcNow,
        Application = _applicationName,
        Environment = _environment,
        Properties = properties ?? new Dictionary<string, string>()
      };

      _logger.LogInformation("Metric: {MetricData}", JsonSerializer.Serialize(metricData));

      // TODO: Send to external monitoring service
      // await SendToExternalMonitoringService(metricData);
    }

    public void TrackDependency(string dependencyType, string dependencyName, string data, DateTime startTime, TimeSpan duration, bool success)
    {
      var dependencyData = new
      {
        DependencyType = dependencyType,
        DependencyName = dependencyName,
        Data = data,
        StartTime = startTime,
        Duration = duration.TotalMilliseconds,
        Success = success,
        Timestamp = DateTime.UtcNow,
        Application = _applicationName,
        Environment = _environment
      };

      _logger.LogInformation("Dependency: {DependencyData}", JsonSerializer.Serialize(dependencyData));

      // TODO: Send to external monitoring service
      // await SendToExternalMonitoringService(dependencyData);
    }

    public void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
    {
      var requestData = new
      {
        RequestName = name,
        StartTime = startTime,
        Duration = duration.TotalMilliseconds,
        ResponseCode = responseCode,
        Success = success,
        Timestamp = DateTime.UtcNow,
        Application = _applicationName,
        Environment = _environment
      };

      _logger.LogInformation("Request: {RequestData}", JsonSerializer.Serialize(requestData));

      // TODO: Send to external monitoring service
      // await SendToExternalMonitoringService(requestData);
    }

    public IDisposable StartOperation(string operationName)
    {
      return new MonitoringOperation(this, operationName);
    }

    private class MonitoringOperation : IDisposable
    {
      private readonly MonitoringService _monitoringService;
      private readonly string _operationName;
      private readonly DateTime _startTime;
      private readonly Stopwatch _stopwatch;

      public MonitoringOperation(MonitoringService monitoringService, string operationName)
      {
        _monitoringService = monitoringService;
        _operationName = operationName;
        _startTime = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();
      }

      public void Dispose()
      {
        _stopwatch.Stop();
        _monitoringService.TrackRequest(_operationName, _startTime, _stopwatch.Elapsed, "200", true);
      }
    }
  }
}
