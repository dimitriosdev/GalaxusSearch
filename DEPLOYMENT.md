# Production Deployment Guide

This guide provides comprehensive instructions for deploying the Galaxus Product Search application to production environments.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Platform-Specific Deployments](#platform-specific-deployments)
3. [Environment Configuration](#environment-configuration)
4. [Database Setup](#database-setup)
5. [Health Checks](#health-checks)
6. [Monitoring](#monitoring)
7. [Security](#security)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

- Docker and Docker Compose
- Node.js 18+ and npm
- .NET 9.0 SDK
- Git

### Required Services

- PostgreSQL database
- Elasticsearch cluster (optional, for enhanced search)
- Redis (optional, for caching)

## Platform-Specific Deployments

### 1. Railway Deployment

#### Quick Start

```bash
# Use the automated deployment script
./deploy.sh railway
```

#### Manual Steps

1. Push code to GitHub repository
2. Sign up for Railway (https://railway.app)
3. Connect Railway to your GitHub account
4. Import your repository
5. Configure environment variables (see below)

Railway will auto-detect and deploy:

- Frontend (Next.js) service
- Backend (.NET API) service
- PostgreSQL database

### 2. Docker Compose (Local/VPS)

```bash
# Development environment
./deploy.sh local

# Production environment
./deploy.sh docker-production
```

### 3. Kubernetes

```bash
# Deploy to Kubernetes cluster
./deploy.sh kubernetes
```

## Environment Configuration

### Backend Environment Variables

Create `backend/appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=galaxus_products;Username=postgres;Password=your_password",
    "ElasticsearchUrl": "http://localhost:9200"
  },
  "Elasticsearch": {
    "DefaultIndex": "products",
    "MaxRetryAttempts": 3,
    "RetryDelayMs": 1000
  },
  "Database": {
    "MaxRetryAttempts": 3,
    "RetryDelayMs": 500
  }
}
```

### Frontend Environment Variables

Create `frontend/.env.production`:

```env
NEXT_PUBLIC_API_URL=https://your-api-domain.com:5119
NEXT_PUBLIC_GRAPHQL_ENDPOINT=https://your-api-domain.com:5119/graphql
NEXT_PUBLIC_ENVIRONMENT=production
NEXT_PUBLIC_MONITORING_ENABLED=true
NEXT_PUBLIC_AUTH_ENABLED=false
NEXT_PUBLIC_API_TIMEOUT=30000
NEXT_PUBLIC_RETRY_ATTEMPTS=3
NEXT_PUBLIC_RETRY_DELAY=1000
```

### Required Environment Variables

| Variable                 | Description                  | Required |
| ------------------------ | ---------------------------- | -------- |
| `DATABASE_URL`           | PostgreSQL connection string | Yes      |
| `ELASTICSEARCH_URL`      | Elasticsearch endpoint       | No       |
| `NEXT_PUBLIC_API_URL`    | Backend API URL              | Yes      |
| `ASPNETCORE_ENVIRONMENT` | Set to "Production"          | Yes      |

## Database Setup

### 1. Run Migrations

The application includes automated database migrations:

```bash
# Using the API endpoints (development)
curl -X POST http://localhost:5119/api/dev/migrate-and-seed

# Or for production environments using direct DB access
psql -h localhost -U postgres -d galaxus -f backend/Database/Migrations/001_InitialSchema.sql
psql -h localhost -U postgres -d galaxus -f backend/Database/Seeds/001_SampleProducts.sql

# Or use Docker to run init script directly
docker exec -i postgres_container psql -U galaxus -d galaxus < init.sql
```

### 2. Database Schema

The migration creates:

- `products` table with comprehensive product information
- Full-text search indices for efficient text searching
- Performance-optimized indexes on category, brand, and price fields
- Support for 10,000+ sample products for testing

### 3. Sample Data

Includes 10,000+ sample products across multiple categories for comprehensive testing, including:

- Electronics, clothing, home goods, books, and more
- Realistic product names, descriptions, and pricing
- Multiple brands and categories for search testing

## Health Checks

The application provides comprehensive health check endpoints:

### Health Check Endpoints

- `GET /health` - Overall application health
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

### Sample Health Check Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "postgres": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "elasticsearch": {
      "status": "Healthy",
      "duration": "00:00:00.0234567"
    }
  }
}
```

## Monitoring

### Application Monitoring

The application includes built-in monitoring capabilities:

- **Error Tracking**: Comprehensive error logging with stack traces
- **Performance Metrics**: Request duration and throughput monitoring
- **User Analytics**: Optional user interaction tracking
- **Health Monitoring**: Automated health checks for all dependencies

### Monitoring Integration

To integrate with external monitoring services:

1. **Sentry** (Error Tracking):

```bash
npm install @sentry/nextjs
```

2. **DataDog** (Application Performance):

```bash
npm install dd-trace
```

3. **LogRocket** (User Session Recording):

```bash
npm install logrocket
```

### Log Aggregation

Logs are structured in JSON format for easy aggregation:

```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "Information",
  "message": "Product search completed",
  "properties": {
    "query": "laptop",
    "results_count": 15,
    "duration_ms": 234
  }
}
```

## Security

### Security Features Implemented

1. **Input Validation**: All GraphQL inputs are validated and sanitized
2. **SQL Injection Prevention**: Using parameterized queries
3. **CORS Configuration**: Properly configured for production
4. **Error Handling**: Secure error messages without sensitive data exposure

### Additional Security Recommendations

1. **HTTPS**: Always use HTTPS in production
2. **Environment Variables**: Never commit secrets to version control
3. **Database Security**: Use connection pooling and proper credentials
4. **Rate Limiting**: Implement rate limiting for API endpoints
5. **Authentication**: Enable authentication when ready (infrastructure prepared)

### Security Headers

Add these headers to your reverse proxy/load balancer:

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

## Troubleshooting

### Common Issues

#### 1. Database Connection Issues

```bash
# Check database connectivity
docker exec -it galaxus-postgres-1 psql -U galaxus -d galaxus -c "SELECT COUNT(*) FROM products;"

# Verify connection string format
# Development: Host=localhost;Database=galaxus;Username=galaxus;Password=galaxus;Port=5432
# Production: Use environment-specific credentials
```

#### 2. Elasticsearch Connection Issues

```bash
# Check Elasticsearch health
curl http://localhost:9200/_cluster/health

# Verify index exists
curl http://localhost:9200/products/_search
```

#### 3. Frontend API Connection Issues

```bash
# Verify backend is running
curl http://localhost:5119/health

# Test GraphQL endpoint
curl -X POST http://localhost:5119/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "{ __schema { types { name } } }"}'

# Test API endpoints
curl -X POST http://localhost:5119/api/sync-elastic
```

### Performance Optimization

1. **Database Indexing**: Ensure proper indexes are created
2. **Connection Pooling**: Configure appropriate pool sizes
3. **Caching**: Implement Redis caching for frequent queries
4. **CDN**: Use CDN for static assets
5. **Load Balancing**: Use multiple backend instances

### Scaling Considerations

1. **Horizontal Scaling**: Deploy multiple backend instances
2. **Database Scaling**: Consider read replicas for heavy read workloads
3. **Search Scaling**: Scale Elasticsearch cluster based on data volume
4. **Frontend Scaling**: Use CDN and multiple regions

## Deployment Checklist

- [ ] Environment variables configured
- [ ] Database migrations run
- [ ] Health checks passing
- [ ] HTTPS configured
- [ ] Monitoring integrated
- [ ] Security headers configured
- [ ] Performance testing completed
- [ ] Backup strategy implemented
- [ ] Rollback plan prepared

## Support

For deployment issues:

1. Check application logs
2. Verify health check endpoints
3. Review environment configuration
4. Test database connectivity
5. Validate API endpoints

## Version Information

- Backend: .NET 9.0
- Frontend: Next.js 15
- Database: PostgreSQL 15+
- Search: Elasticsearch 8+ (optional)
