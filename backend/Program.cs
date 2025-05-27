using Backend.Api.GraphQL;
using Backend.Api.Services;
using Backend.Api.Services.Strategies;
using Backend.Api.HealthChecks;
using Backend.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Health checks with custom checks for dependencies
builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres", tags: new[] { "database", "ready" })
    .AddCheck<ElasticsearchHealthCheck>("elasticsearch", tags: new[] { "search", "ready" });

// Register monitoring service
builder.Services.AddSingleton<IMonitoringService, MonitoringService>();

// Configuration with validation
var elasticUri = builder.Configuration["Elasticsearch:Uri"] ??
    Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ??
    "http://localhost:9200";
var postgresConnStr = builder.Configuration.GetConnectionString("DefaultConnection") ??
    Environment.GetEnvironmentVariable("DATABASE_URL") ??
    "Host=localhost;Port=5432;Database=galaxus;Username=galaxus;Password=galaxus";

// Validate required configuration
if (string.IsNullOrEmpty(elasticUri))
  throw new InvalidOperationException("Elasticsearch URI is required");
if (string.IsNullOrEmpty(postgresConnStr))
  throw new InvalidOperationException("Database connection string is required");

// Register services with proper DI
builder.Services.AddSingleton<IProductRepository>(sp =>
    new PostgresProductRepository(postgresConnStr, sp.GetRequiredService<ILogger<PostgresProductRepository>>()));
builder.Services.AddSingleton<IDatabaseMigrationService>(sp =>
    new DatabaseMigrationService(postgresConnStr, sp.GetRequiredService<ILogger<DatabaseMigrationService>>()));
builder.Services.AddSingleton<IProductSearchService>(sp =>
    new ElasticsearchService(elasticUri, sp.GetRequiredService<IProductRepository>(), sp.GetRequiredService<ILogger<ElasticsearchService>>(), sp.GetRequiredService<IMonitoringService>()));

// Register health check dependencies
builder.Services.AddSingleton<PostgresHealthCheck>(sp => new PostgresHealthCheck(postgresConnStr));
builder.Services.AddSingleton<ElasticsearchHealthCheck>(sp =>
{
  var elasticsearchService = sp.GetRequiredService<IProductSearchService>() as ElasticsearchService;
  return new ElasticsearchHealthCheck(elasticsearchService!.Client);
});

// GraphQL with error handling and security
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(opt =>
    {
      opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
      opt.ExecutionTimeout = TimeSpan.FromSeconds(30);
    });

// Add CORS services with proper configuration
builder.Services.AddCors(options =>
{
  options.AddPolicy("ProductionCors", policy =>
  {
    if (builder.Environment.IsDevelopment())
    {
      policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    }
    else
    {
      var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "https://yourdomain.com" };
      policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowCredentials()
              .WithMethods("GET", "POST");
    }
  });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();

  // Development endpoints for database operations
  app.MapPost("/dev/migrate", async (IDatabaseMigrationService migrationService) =>
  {
    await migrationService.MigrateAsync();
    return Results.Ok("Database migration completed");
  });

  app.MapPost("/dev/seed", async (IDatabaseMigrationService migrationService) =>
  {
    await migrationService.SeedAsync();
    return Results.Ok("Database seeding completed");
  });

  app.MapPost("/dev/migrate-and-seed", async (IDatabaseMigrationService migrationService) =>
  {
    await migrationService.MigrateAsync();
    await migrationService.SeedAsync();
    return Results.Ok("Database migration and seeding completed");
  });
}
else
{
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();

// Add monitoring middleware
app.UseMiddleware<MonitoringMiddleware>();

// Add security headers
app.Use(async (context, next) =>
{
  context.Response.Headers["X-Content-Type-Options"] = "nosniff";
  context.Response.Headers["X-Frame-Options"] = "DENY";
  context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
  context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
  await next();
});

// Enable CORS
app.UseCors("ProductionCors");

// Health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
  Predicate = _ => true,
  ResponseWriter = async (context, report) =>
  {
    context.Response.ContentType = "application/json";
    var response = new
    {
      status = report.Status.ToString(),
      checks = report.Entries.Select(x => new
      {
        name = x.Key,
        status = x.Value.Status.ToString(),
        description = x.Value.Description,
        duration = x.Value.Duration.ToString()
      }),
      totalDuration = report.TotalDuration.ToString()
    };
    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
  }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
  Predicate = _ => false // Just return 200 OK for liveness
});

// Health checks
app.MapHealthChecks("/health");

app.MapGraphQL();

// Add a secure REST endpoint to trigger sync from PostgreSQL to Elasticsearch
app.MapPost("/api/sync-elastic", (IProductSearchService esService) =>
{
  try
  {
    esService.SyncProductsFromPostgres(postgresConnStr);
    return Results.Ok(new { message = "Sync completed successfully", timestamp = DateTime.UtcNow });
  }
  catch (Exception ex)
  {
    return Results.Problem(
        detail: app.Environment.IsDevelopment() ? ex.Message : "An error occurred during sync",
        statusCode: 500);
  }
});

app.Run();
