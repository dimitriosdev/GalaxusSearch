# Architecture Documentation

## 🏗️ **System Architecture Overview**

The Galaxus Product Search application follows a modern, layered architecture that promotes separation of concerns, testability, and scalability. This document provides a comprehensive view of the architectural decisions and patterns implemented.

## 📐 **Architectural Patterns**

### **Clean Architecture Implementation**

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[Next.js Frontend]
        GraphQL[GraphQL API]
        REST[REST Endpoints]
    end

    subgraph "Application Layer"
        Queries[GraphQL Queries]
        Commands[API Commands]
        Handlers[Request Handlers]
    end

    subgraph "Domain Layer"
        Entities[Domain Entities]
        Services[Domain Services]
        Interfaces[Repository Interfaces]
    end

    subgraph "Infrastructure Layer"
        Repos[Repository Implementations]
        ES[Elasticsearch Client]
        PG[PostgreSQL Context]
        External[External Services]
    end

    UI --> GraphQL
    UI --> REST
    GraphQL --> Queries
    REST --> Commands
    Queries --> Handlers
    Commands --> Handlers
    Handlers --> Services
    Services --> Interfaces
    Interfaces --> Repos
    Repos --> ES
    Repos --> PG
    Services --> External
```

### **SOLID Principles Application**

#### **Single Responsibility Principle**

Each class has a single, well-defined responsibility:

```csharp
// ✅ Single responsibility - only handles PostgreSQL data access
public class PostgresProductRepository : IProductRepository
{
    // Only contains database access logic
}

// ✅ Single responsibility - only handles search operations
public class ElasticsearchService : IProductSearchService
{
    // Only contains search logic
}

// ✅ Single responsibility - only handles monitoring
public class MonitoringService : IMonitoringService
{
    // Only contains logging and metrics logic
}
```

#### **Open/Closed Principle**

The system is open for extension but closed for modification:

```csharp
// ✅ Interface allows extension without modification
public interface ISearchStrategy
{
    ISearchResponse<Product> Search(/* parameters */);
}

// ✅ New search strategies can be added without changing existing code
public class ElasticsearchSearchStrategy : ISearchStrategy { }
public class SolrSearchStrategy : ISearchStrategy { } // Future extension
```

#### **Liskov Substitution Principle**

Derived classes are substitutable for their base classes:

```csharp
// ✅ Any IProductRepository implementation can be substituted
IProductRepository repo = new PostgresProductRepository();
// OR
IProductRepository repo = new InMemoryProductRepository(); // For testing
```

#### **Interface Segregation Principle**

Interfaces are client-specific and focused:

```csharp
// ✅ Focused interfaces instead of monolithic ones
public interface IProductRepository { /* Read/Write operations */ }
public interface IProductSearchService { /* Search operations */ }
public interface IMonitoringService { /* Monitoring operations */ }
```

#### **Dependency Inversion Principle**

High-level modules don't depend on low-level modules:

```csharp
// ✅ Depends on abstraction, not concrete implementation
public class Query
{
    private readonly IProductSearchService _searchService;

    // Dependency injected, not instantiated
    public Query(IProductSearchService searchService)
    {
        _searchService = searchService;
    }
}
```

## 🔄 **Data Flow Architecture**

### **Request Processing Flow**

```mermaid
sequenceDiagram
    participant Client
    participant API as GraphQL API
    participant Service as Search Service
    participant Strategy as Search Strategy
    participant ES as Elasticsearch
    participant Monitor as Monitoring

    Client->>API: GraphQL Query
    API->>Monitor: Log Request Start
    API->>Service: SearchProducts()
    Service->>Strategy: Execute Search
    Strategy->>ES: Query Index
    ES-->>Strategy: Search Results
    Strategy-->>Service: Processed Results
    Service->>Monitor: Log Performance Metrics
    Service-->>API: Product DTOs
    API->>Monitor: Log Request Complete
    API-->>Client: GraphQL Response
```

### **Data Synchronization Flow**

```mermaid
graph LR
    subgraph "Data Sources"
        PG[(PostgreSQL<br/>Source of Truth)]
        Files[CSV/JSON Files]
        API[External APIs]
    end

    subgraph "Sync Layer"
        Sync[Sync Service]
        Queue[Message Queue]
        Scheduler[Job Scheduler]
    end

    subgraph "Search Layer"
        ES[(Elasticsearch<br/>Search Index)]
        Cache[Redis Cache]
    end

    PG --> Sync
    Files --> Sync
    API --> Sync
    Sync --> Queue
    Queue --> ES
    Scheduler --> Sync
    ES --> Cache
```

## 🧱 **Component Architecture**

### **Frontend Architecture**

```mermaid
graph TB
    subgraph "Next.js Application"
        Pages[App Router Pages]
        Components[UI Components]
        Hooks[Custom Hooks]
        Utils[Utility Functions]
    end

    subgraph "State Management"
        Apollo[Apollo Client Cache]
        Local[Local State]
        Context[React Context]
    end

    subgraph "Data Layer"
        GraphQL[GraphQL Queries]
        REST[REST API Calls]
        Types[TypeScript Types]
    end

    Pages --> Components
    Components --> Hooks
    Hooks --> Apollo
    Hooks --> Local
    Components --> Context
    Hooks --> GraphQL
    Utils --> REST
    Types --> GraphQL
    Types --> REST
```

### **Backend Architecture**

```mermaid
graph TB
    subgraph "API Layer"
        GraphQL[HotChocolate GraphQL]
        Middleware[Custom Middleware]
        Filters[Exception Filters]
    end

    subgraph "Business Logic"
        Services[Domain Services]
        Strategies[Strategy Pattern]
        Validators[Input Validators]
    end

    subgraph "Data Access"
        Repositories[Repository Pattern]
        EF[Entity Framework]
        ES[Elasticsearch Client]
    end

    subgraph "Infrastructure"
        Health[Health Checks]
        Logging[Structured Logging]
        Config[Configuration]
    end

    GraphQL --> Services
    Middleware --> GraphQL
    Filters --> GraphQL
    Services --> Strategies
    Services --> Validators
    Strategies --> Repositories
    Repositories --> EF
    Repositories --> ES
    Health --> EF
    Health --> ES
    Logging --> Services
    Config --> Services
```

## 🏗️ **Design Patterns Implementation**

### **Repository Pattern**

Abstracts data access logic and provides a uniform interface:

```csharp
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProductsAsync(int page, int size);
    Task<Product?> GetProductByIdAsync(string id);
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string id);
}

public class PostgresProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    // Implementation uses Entity Framework for data access
}
```

### **Strategy Pattern**

Enables different search implementations to be used interchangeably:

```csharp
public interface ISearchStrategy
{
    ISearchResponse<Product> Search(
        string? query,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        int size = 1000);
}

public class ElasticsearchSearchStrategy : ISearchStrategy
{
    // Elasticsearch-specific search implementation
}

public class PostgresSearchStrategy : ISearchStrategy
{
    // PostgreSQL-specific search implementation (fallback)
}
```

### **Factory Pattern**

Creates appropriate strategy instances based on configuration:

```csharp
public class SearchStrategyFactory
{
    public ISearchStrategy CreateStrategy(SearchType type)
    {
        return type switch
        {
            SearchType.Elasticsearch => new ElasticsearchSearchStrategy(),
            SearchType.Postgres => new PostgresSearchStrategy(),
            _ => throw new ArgumentException("Invalid search type")
        };
    }
}
```

### **Observer Pattern**

Implements monitoring and logging across the application:

```csharp
public interface IMonitoringService
{
    void LogUserAction(string action, object metadata);
    void LogError(Exception error, string context, object metadata);
    void LogPerformance(string metric, double value, object metadata);
}

// Components observe and report to monitoring service
public class SearchService
{
    private readonly IMonitoringService _monitoring;

    public async Task<SearchResult> SearchAsync(string query)
    {
        _monitoring.LogUserAction("search_initiated", new { query });
        // ... search logic
        _monitoring.LogPerformance("search_duration", duration);
    }
}
```

## 🔐 **Security Architecture**

### **Security Layers**

```mermaid
graph TB
    subgraph "Frontend Security"
        CSP[Content Security Policy]
        XSS[XSS Protection]
        HTTPS[HTTPS Enforcement]
    end

    subgraph "API Security"
        CORS[CORS Configuration]
        RateLimit[Rate Limiting]
        Validation[Input Validation]
    end

    subgraph "Data Security"
        Encryption[Data Encryption]
        Access[Access Control]
        Audit[Audit Logging]
    end

    subgraph "Infrastructure Security"
        Network[Network Security]
        Secrets[Secret Management]
        Monitoring[Security Monitoring]
    end
```

### **Input Validation Architecture**

```csharp
public static class InputValidator
{
    private static readonly Regex QuerySanitizationRegex =
        new(@"[<>""'&]", RegexOptions.Compiled);

    public static void ValidateSearchParameters(
        string? query,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        int size)
    {
        // Comprehensive validation with business rules
        if (size <= 0 || size > 1000)
            throw new ArgumentException("Size must be between 1 and 1000");

        if (!string.IsNullOrEmpty(query) && query.Length > 100)
            throw new ArgumentException("Query cannot exceed 100 characters");

        // Category validation against allowed values
        if (!string.IsNullOrEmpty(category) &&
            !ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Invalid category");
    }
}
```

## 📊 **Performance Architecture**

### **Caching Strategy**

```mermaid
graph TB
    subgraph "Frontend Caching"
        Apollo[Apollo Client Cache]
        Browser[Browser Cache]
        CDN[CDN Cache]
    end

    subgraph "Backend Caching"
        Memory[In-Memory Cache]
        Redis[Redis Cache]
        Database[Database Cache]
    end

    subgraph "Search Caching"
        ESCache[Elasticsearch Cache]
        QueryCache[Query Result Cache]
        Aggregation[Aggregation Cache]
    end

    Apollo --> Memory
    Browser --> CDN
    Memory --> Redis
    Redis --> Database
    ESCache --> QueryCache
    QueryCache --> Aggregation
```

### **Connection Pooling**

```csharp
// PostgreSQL connection pooling configuration
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
}, ServiceLifetime.Scoped);

// Elasticsearch connection pooling
services.AddSingleton<ElasticClient>(provider =>
{
    var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
        .DefaultIndex("products")
        .EnableApiVersioningHeader()
        .MaximumRetries(3)
        .MaxRetryTimeout(TimeSpan.FromSeconds(30));

    return new ElasticClient(settings);
});
```

## 🔄 **Scalability Architecture**

### **Horizontal Scaling Strategy**

```mermaid
graph TB
    subgraph "Load Balancer Layer"
        LB[Load Balancer]
        WAF[Web Application Firewall]
    end

    subgraph "Application Layer"
        API1[API Instance 1]
        API2[API Instance 2]
        API3[API Instance N]
    end

    subgraph "Data Layer"
        PGMaster[(PostgreSQL Master)]
        PGReplica1[(PostgreSQL Replica 1)]
        PGReplica2[(PostgreSQL Replica 2)]
        ESNode1[(ES Node 1)]
        ESNode2[(ES Node 2)]
        ESNode3[(ES Node 3)]
    end

    LB --> API1
    LB --> API2
    LB --> API3

    API1 --> PGMaster
    API2 --> PGReplica1
    API3 --> PGReplica2

    API1 --> ESNode1
    API2 --> ESNode2
    API3 --> ESNode3
```

### **Microservices Evolution Path**

```mermaid
graph TB
    subgraph "Current Monolith"
        Mono[Galaxus API]
    end

    subgraph "Future Microservices"
        Search[Search Service]
        Product[Product Service]
        User[User Service]
        Order[Order Service]
    end

    subgraph "Shared Infrastructure"
        Gateway[API Gateway]
        ServiceMesh[Service Mesh]
        MessageBus[Message Bus]
    end

    Mono -.->|Evolution| Search
    Mono -.->|Evolution| Product
    Mono -.->|Evolution| User
    Mono -.->|Evolution| Order

    Gateway --> Search
    Gateway --> Product
    Gateway --> User
    Gateway --> Order

    ServiceMesh --> Search
    ServiceMesh --> Product
    ServiceMesh --> User
    ServiceMesh --> Order

    MessageBus --> Search
    MessageBus --> Product
    MessageBus --> User
    MessageBus --> Order
```

## 🧪 **Testing Architecture**

### **Testing Pyramid**

```mermaid
graph TB
    subgraph "Testing Levels"
        E2E[End-to-End Tests<br/>5%]
        Integration[Integration Tests<br/>25%]
        Unit[Unit Tests<br/>70%]
    end

    subgraph "Testing Tools"
        Playwright[Playwright/Cypress]
        TestHost[TestWebApplicationFactory]
        xUnit[xUnit + Moq]
    end

    E2E --> Playwright
    Integration --> TestHost
    Unit --> xUnit
```

### **Test Strategy Implementation**

```csharp
// Unit Test Example
[Fact]
public async Task SearchProducts_ValidQuery_ReturnsProducts()
{
    // Arrange
    var mockRepository = new Mock<IProductRepository>();
    var service = new ProductSearchService(mockRepository.Object);

    // Act
    var result = await service.SearchAsync("laptop");

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Products.Any());
}

// Integration Test Example
public class SearchIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task GraphQL_SearchProducts_ReturnsValidResponse()
    {
        // Full integration test with real database
    }
}
```

This architecture provides a solid foundation for a scalable, maintainable, and testable e-commerce search platform that can evolve with changing requirements and growing user demands.
