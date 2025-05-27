# API Documentation

## 🚀 **Overview**

The Galaxus Product Search API provides both GraphQL and REST endpoints for searching and managing products. This document covers all available endpoints, their parameters, responses, and usage examples.

## 📊 **GraphQL API**

### **Endpoint**

```
POST http://localhost:5119/graphql
```

### **Schema Overview**

```graphql
type Query {
  searchProducts(
    query: String
    category: String
    minPrice: Float
    maxPrice: Float
    size: Int = 20
  ): [Product!]!

  getProduct(id: String!): Product
}

type Product {
  id: String!
  name: String!
  description: String!
  price: Float!
  category: String!
  brand: String!
  sku: String!
  stock: Int!
  createdAt: String!
}
```

### **Query Examples**

#### **Basic Product Search**

```graphql
query SearchProducts {
  searchProducts(size: 10) {
    id
    name
    price
    category
  }
}
```

**Response:**

```json
{
  "data": {
    "searchProducts": [
      {
        "id": "1",
        "name": "Product 1",
        "price": 99.99,
        "category": "Electronics"
      }
    ]
  }
}
```

#### **Search with Filters**

```graphql
query SearchWithFilters {
  searchProducts(
    query: "laptop"
    category: "Electronics"
    minPrice: 500.0
    maxPrice: 2000.0
    size: 20
  ) {
    id
    name
    description
    price
    category
    brand
    sku
    stock
  }
}
```

**Response:**

```json
{
  "data": {
    "searchProducts": [
      {
        "id": "123",
        "name": "Gaming Laptop Pro",
        "description": "High-performance gaming laptop with RTX graphics",
        "price": 1599.99,
        "category": "Electronics",
        "brand": "TechBrand",
        "sku": "LAP-123-PRO",
        "stock": 15
      }
    ]
  }
}
```

#### **Get Single Product**

```graphql
query GetProduct {
  getProduct(id: "123") {
    id
    name
    description
    price
    category
    brand
    sku
    stock
    createdAt
  }
}
```

### **Error Handling**

GraphQL errors are returned in the standard format:

```json
{
  "errors": [
    {
      "message": "Invalid category. Valid categories are: Electronics, Books, Automotive, Furniture, Garden, Home Appliances, Sportswear, Toys",
      "extensions": {
        "code": "INVALID_ARGUMENT"
      }
    }
  ]
}
```

### **Input Validation Rules**

| Parameter  | Validation Rules                                                                                     |
| ---------- | ---------------------------------------------------------------------------------------------------- |
| `query`    | Max 100 characters, no HTML/script tags                                                              |
| `category` | Must be one of: Electronics, Books, Automotive, Furniture, Garden, Home Appliances, Sportswear, Toys |
| `minPrice` | Must be >= 0                                                                                         |
| `maxPrice` | Must be >= 0, <= 100,000                                                                             |
| `size`     | Must be between 1 and 1000                                                                           |

## 🔄 **REST API Endpoints**

### **Health Check Endpoints**

#### **Overall Health**

```http
GET /health
```

**Response (Healthy):**

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:01.234",
  "entries": {
    "PostgreSQL": {
      "status": "Healthy",
      "description": "PostgreSQL database is healthy",
      "duration": "00:00:00.123"
    },
    "Elasticsearch": {
      "status": "Healthy",
      "description": "Elasticsearch is healthy",
      "duration": "00:00:00.234"
    }
  }
}
```

**Response (Unhealthy):**

```json
{
  "status": "Unhealthy",
  "totalDuration": "00:00:05.000",
  "entries": {
    "PostgreSQL": {
      "status": "Unhealthy",
      "description": "PostgreSQL database is unhealthy",
      "exception": "Connection timeout",
      "duration": "00:00:05.000"
    }
  }
}
```

#### **Readiness Check**

```http
GET /health/ready
```

Returns 200 if all services are ready to handle requests.

#### **Liveness Check**

```http
GET /health/live
```

Returns 200 if the application is running (lighter check).

### **Data Synchronization Endpoints**

#### **Sync Products to Elasticsearch**

```http
POST /api/sync-elastic
```

**Response:**

```json
{
  "message": "Elasticsearch sync completed successfully",
  "syncedCount": 10003,
  "duration": "00:00:15.234"
}
```

## 🛠️ **Client Usage Examples**

### **JavaScript/TypeScript with Apollo Client**

```typescript
import { ApolloClient, InMemoryCache, gql } from "@apollo/client";

const client = new ApolloClient({
  uri: "http://localhost:5119/graphql",
  cache: new InMemoryCache(),
});

const SEARCH_PRODUCTS = gql`
  query SearchProducts(
    $query: String
    $category: String
    $minPrice: Float
    $maxPrice: Float
    $size: Int
  ) {
    searchProducts(
      query: $query
      category: $category
      minPrice: $minPrice
      maxPrice: $maxPrice
      size: $size
    ) {
      id
      name
      price
      category
      brand
    }
  }
`;

// Usage
const { data, loading, error } = useQuery(SEARCH_PRODUCTS, {
  variables: {
    query: "laptop",
    category: "Electronics",
    minPrice: 500,
    maxPrice: 2000,
    size: 20,
  },
});
```

### **React Hook Implementation**

```typescript
import { useCallback, useState } from "react";
import { useQuery } from "@apollo/client";

interface UseProductsConfig {
  query: string;
  category: string;
  minPrice?: string;
  maxPrice?: string;
  size?: number;
}

export function useProducts(config: UseProductsConfig) {
  const { data, loading, error, refetch } = useQuery(SEARCH_PRODUCTS, {
    variables: {
      query: config.query || undefined,
      category: config.category || undefined,
      minPrice: config.minPrice ? parseFloat(config.minPrice) : undefined,
      maxPrice: config.maxPrice ? parseFloat(config.maxPrice) : undefined,
      size: config.size || 20,
    },
    skip: !config.query && !config.category,
  });

  return {
    products: data?.searchProducts || [],
    loading,
    error: error?.message,
    refetch,
  };
}
```

### **cURL Examples**

#### **GraphQL Query**

```bash
curl -X POST http://localhost:5119/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "query { searchProducts(query: \"laptop\", size: 5) { id name price category } }"
  }'
```

#### **PowerShell (Windows)**

```powershell
$query = '{"query": "query { searchProducts(query: \"laptop\", size: 5) { id name price category } }"}'
Invoke-WebRequest -Uri "http://localhost:5119/graphql" -Method POST -Headers @{"Content-Type"="application/json"} -Body $query
```

#### **Health Check**

```bash
curl -X GET http://localhost:5119/health
```

### **Python with requests**

```python
import requests

# GraphQL Query
def search_products(query=None, category=None, min_price=None, max_price=None, size=20):
    graphql_query = """
    query SearchProducts($query: String, $category: String, $minPrice: Float, $maxPrice: Float, $size: Int) {
      searchProducts(query: $query, category: $category, minPrice: $minPrice, maxPrice: $maxPrice, size: $size) {
        id
        name
        price
        category
        brand
      }
    }
    """

    variables = {
        "query": query,
        "category": category,
        "minPrice": min_price,
        "maxPrice": max_price,
        "size": size
    }

    response = requests.post(
        "http://localhost:5119/graphql",
        json={"query": graphql_query, "variables": variables},
        headers={"Content-Type": "application/json"}
    )

    return response.json()

# Usage
result = search_products(query="laptop", category="Electronics", size=10)
products = result["data"]["searchProducts"]
```

## 📝 **API Rate Limiting**

The API implements rate limiting to ensure fair usage:

| Endpoint Type  | Rate Limit   | Window   |
| -------------- | ------------ | -------- |
| GraphQL        | 100 requests | 1 minute |
| Health Checks  | 60 requests  | 1 minute |
| Sync Endpoints | 5 requests   | 1 minute |

Rate limit headers are included in responses:

```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640995200
```

## 🔐 **Authentication & Authorization**

Currently, the API operates without authentication for simplicity. For production deployment, consider implementing:

### **Future Authentication Options**

1. **JWT Token Authentication**

```http
Authorization: Bearer <jwt-token>
```

2. **API Key Authentication**

```http
X-API-Key: <your-api-key>
```

3. **OAuth 2.0 / OpenID Connect**

```http
Authorization: Bearer <oauth-token>
```

### **Role-Based Access Control (Future)**

```typescript
enum Role {
  ADMIN = "admin",
  USER = "user",
  READONLY = "readonly",
}

// Different access levels for different roles
interface Permission {
  role: Role;
  canRead: boolean;
  canWrite: boolean;
  canDelete: boolean;
}
```

## 🚨 **Error Codes & Messages**

### **Common Error Responses**

| Error Code            | HTTP Status | Description                  |
| --------------------- | ----------- | ---------------------------- |
| `INVALID_ARGUMENT`    | 400         | Invalid input parameters     |
| `NOT_FOUND`           | 404         | Resource not found           |
| `RATE_LIMITED`        | 429         | Too many requests            |
| `INTERNAL_ERROR`      | 500         | Internal server error        |
| `SERVICE_UNAVAILABLE` | 503         | External service unavailable |

### **GraphQL Error Extensions**

```json
{
  "errors": [
    {
      "message": "Size must be between 1 and 1000",
      "extensions": {
        "code": "INVALID_ARGUMENT",
        "field": "size",
        "value": 5000
      }
    }
  ]
}
```

## 📊 **Performance Considerations**

### **Query Optimization Tips**

1. **Use specific field selection**

```graphql
# ✅ Good - only request needed fields
query {
  searchProducts(size: 10) {
    id
    name
    price
  }
}

# ❌ Avoid - requesting all fields unnecessarily
query {
  searchProducts(size: 10) {
    id
    name
    description
    price
    category
    brand
    sku
    stock
    createdAt
  }
}
```

2. **Limit result size appropriately**

```graphql
# ✅ Good - reasonable page size
searchProducts(size: 20)

# ❌ Avoid - large result sets
searchProducts(size: 1000)
```

3. **Use filters to narrow results**

```graphql
# ✅ Good - filtered search
searchProducts(query: "laptop", category: "Electronics", minPrice: 500)

# ❌ Less efficient - unfiltered large search
searchProducts(size: 500)
```

## 🔄 **API Versioning Strategy**

Currently using implicit versioning through the GraphQL schema. Future versioning options:

### **URL Versioning**

```http
POST /v1/graphql
POST /v2/graphql
```

### **Header Versioning**

```http
API-Version: v1
```

### **GraphQL Schema Versioning**

```graphql
type Query {
  searchProducts: [Product!]! @deprecated(reason: "Use searchProductsV2")
  searchProductsV2: SearchResult!
}
```

This API documentation provides comprehensive coverage of all available endpoints and their usage patterns, enabling developers to effectively integrate with the Galaxus Product Search platform.
