using Backend.Api.GraphQL;
using Backend.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var elasticUri = "http://localhost:9200";
var postgresConnStr = builder.Configuration.GetConnectionString("DefaultConnection") ??
    "Host=localhost;Port=5432;Database=galaxus;Username=galaxus;Password=galaxus";
builder.Services.AddSingleton<IProductRepository>(new PostgresProductRepository(postgresConnStr));
builder.Services.AddSingleton<IProductSearchService>(sp =>
    new ElasticsearchService(elasticUri, sp.GetRequiredService<IProductRepository>()));
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();

// Add CORS services
builder.Services.AddCors();

var app = builder.Build();

// Enable CORS for frontend
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod()
);

app.MapGraphQL();

// Add a minimal REST endpoint to trigger sync from PostgreSQL to Elasticsearch
app.MapPost("/sync-elastic", (IProductSearchService esService) =>
{
  esService.SyncProductsFromPostgres(postgresConnStr);
  return Results.Ok("Sync complete");
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
