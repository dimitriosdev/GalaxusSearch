# Galaxus Product Search: Complete Developer Setup Guide

This comprehensive guide will help you get the full-stack Galaxus Product Search application running locally from scratch. The application includes a .NET 9 GraphQL API backend, Next.js frontend with Apollo Client, PostgreSQL database, and Elasticsearch for advanced search capabilities.

---

## 🛠️ Prerequisites

Before starting, ensure you have the following installed:

- **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** - For PostgreSQL and Elasticsearch services
- **[.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)** - For the backend API
- **[Node.js](https://nodejs.org/) (v18+)** and **npm** - For the frontend application
- **[Git](https://git-scm.com/)** - For version control

### Verify Prerequisites

Run these commands in PowerShell to verify your setup:

```powershell
# Check Docker
docker --version
docker compose version

# Check .NET
dotnet --version

# Check Node.js and npm
node --version
npm --version

# Check Git
git --version
```

---

## 🚀 Quick Start (5 Steps)

### Step 1: Clone and Navigate to Project

```powershell
# If you haven't cloned the repository yet
git clone <repository-url>
cd galaxus
```

### Step 2: Start Database & Search Services

From the project root directory:

```powershell
# Start PostgreSQL and Elasticsearch in background
docker compose up -d

# Verify services are running
docker compose ps
```

This will start:

- **PostgreSQL** on port 5432 (auto-seeded with 10,000+ products)
- **Elasticsearch** on port 9200 (for search functionality)

### Step 3: Start the Backend API (.NET)

Open a new PowerShell terminal and run:

```powershell
# Navigate to backend directory
cd backend

# Restore NuGet packages
dotnet restore

# Start the API in development mode
dotnet run
```

The backend will start on [http://localhost:5119](http://localhost:5119) with:

- GraphQL endpoint at `/graphql`
- Health check endpoint at `/health`
- API endpoints at `/api/*`
- Development utilities (migration, seeding)

### Step 4: Start the Frontend (Next.js)

Open another PowerShell terminal and run:

```powershell
# Navigate to frontend directory
cd frontend

# Install npm dependencies
npm install

# Start the development server with Turbopack
npm run dev
```

The frontend will start on [http://localhost:3000](http://localhost:3000) with:

> **Note**: If port 3000 is in use, Next.js will automatically use the next available port (e.g., 3001). Check your terminal output for the actual URL.

- Hot reload enabled
- Turbopack for faster builds
- TypeScript support

### Step 5: Sync Products to Elasticsearch

After the backend is running, enable search functionality:

**Option 1: Using PowerShell's Invoke-WebRequest (recommended)**

```powershell
Invoke-WebRequest -Uri "http://localhost:5119/api/sync-elastic" -Method POST
```

**Option 2: Using curl.exe directly**

```powershell
curl.exe -X POST http://localhost:5119/api/sync-elastic
```

**Option 3: Initialize database first (if needed)**

```powershell
# Initialize database with sample data
Invoke-WebRequest -Uri "http://localhost:5119/api/dev/migrate-and-seed" -Method POST
```

This indexes all products from PostgreSQL into Elasticsearch for enhanced search capabilities.

---

## 🧪 Running Tests

The project includes comprehensive test suites:

### Backend Tests

```powershell
cd backend
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests

```powershell
cd frontend
# Run all tests
npm test

# Run tests in watch mode
npm run test:watch

# Run with coverage
npm run test:coverage
```

### Comprehensive Test Suite

For full system testing (requires bash/WSL on Windows):

```bash
# Run all tests including integration and performance tests
./test.sh
```

---

## 🔧 Development Features

### Backend Features

- **GraphQL API** with HotChocolate
- **PostgreSQL** with Dapper for data access
- **Elasticsearch** integration for search
- **Health checks** and monitoring
- **Serilog** for structured logging
- **FluentValidation** for input validation
- **Polly** for resilience patterns
- **Development endpoints** for database migration and seeding

### Frontend Features

- **Next.js 15** with Turbopack
- **React 19** with TypeScript
- **Apollo Client** for GraphQL
- **Tailwind CSS** for styling
- **Jest** and Testing Library for testing
- **ESLint** for code quality
- **Real-time search** with debouncing and error handling

---

## 📋 Available Services & Endpoints

### Frontend

- **Main Application**: [http://localhost:3000](http://localhost:3000)
- **Product Search**: Real-time search with Apollo Client
- **Responsive Design**: Mobile-friendly interface

### Backend API

- **Base URL**: [http://localhost:5119](http://localhost:5119)
- **GraphQL Playground**: [http://localhost:5119/graphql](http://localhost:5119/graphql)
- **Health Check**: [http://localhost:5119/health](http://localhost:5119/health)
- **Sync Endpoint**: `POST http://localhost:5119/api/sync-elastic`
- **Dev Migration**: `POST http://localhost:5119/api/dev/migrate-and-seed`

### Database Services

- **PostgreSQL**: `localhost:5432`
  - Database: `galaxus`
  - Username: `galaxus`
  - Password: `galaxus`
- **Elasticsearch**: [http://localhost:9200](http://localhost:9200)
  - Cluster health: [http://localhost:9200/\_cluster/health](http://localhost:9200/_cluster/health)

---

## 🛑 Stopping Services

### Stop Application Servers

- Press `Ctrl+C` in the terminals running the backend and frontend

### Stop Database Services

```powershell
# Stop and remove containers
docker compose down

# Stop and remove containers + volumes (removes all data)
docker compose down -v
```

---

## 🔍 Troubleshooting

### Common Issues and Solutions

#### Docker Issues

```powershell
# Check if Docker Desktop is running
docker --version

# View running containers
docker compose ps

# View logs for specific service
docker compose logs postgres
docker compose logs elasticsearch

# Restart services
docker compose restart
```

#### Backend Issues

```powershell
# Check if backend is running
Invoke-WebRequest -Uri "http://localhost:5119/health" -Method GET

# Initialize database if needed
Invoke-WebRequest -Uri "http://localhost:5119/api/dev/migrate-and-seed" -Method POST

# View backend logs (if running in terminal)
# Check the terminal where you ran 'dotnet run'

# Clean and rebuild
dotnet clean
dotnet build
```

#### Frontend Issues

```powershell
# Clear npm cache and reinstall
npm cache clean --force
Remove-Item -Recurse -Force node_modules
npm install

# Check if frontend is accessible
Invoke-WebRequest -Uri "http://localhost:3000" -Method GET
```

#### Database Connection Issues

- Verify PostgreSQL is running: `docker compose ps`
- Check connection string in `backend/appsettings.Development.json`
- Ensure port 5432 is not used by another service
- **Initialize database if empty**: Use the migration endpoint if you see database-related errors:
  ```powershell
  Invoke-WebRequest -Uri "http://localhost:5119/api/dev/migrate-and-seed" -Method POST
  ```

#### Elasticsearch Issues

- Verify Elasticsearch is running: `Invoke-WebRequest -Uri "http://localhost:9200" -Method GET`
- Check if port 9200 is available
- For search issues, ensure you've run the sync command
- **Sync products to Elasticsearch**:
  ```powershell
  Invoke-WebRequest -Uri "http://localhost:5119/api/sync-elastic" -Method POST
  ```

#### Common Error Messages

**"Table 'products' doesn't exist"**

- Run database migration: `Invoke-WebRequest -Uri "http://localhost:5119/api/dev/migrate-and-seed" -Method POST`

**"Health check failed"**

- Backend might be starting up - wait 30-60 seconds and try again
- Check if all Docker services are running: `docker compose ps`

**"GraphQL infinite calls"**

- This has been fixed in the current version with proper useCallback dependencies
- If you see this, ensure you have the latest frontend code

#### Port Conflicts

```powershell
# Check what's using specific ports
netstat -an | findstr :5119
netstat -an | findstr :3000
netstat -an | findstr :5432
netstat -an | findstr :9200
```

---

## 📁 Project Structure

```
galaxus/
├── backend/                    # .NET 9 Web API
│   ├── Services/              # Business logic and data access
│   ├── GraphQL/               # GraphQL schema and resolvers
│   ├── Models/                # Data models
│   ├── DTOs/                  # Data transfer objects
│   ├── Database/              # Migrations and seeds
│   └── HealthChecks/          # Health monitoring
├── frontend/                  # Next.js React application
│   ├── src/app/               # App router pages
│   ├── src/components/        # React components
│   ├── src/hooks/             # Custom React hooks
│   └── pages/                 # Pages router (legacy)
├── Backend.Tests/             # Backend test suite
│   ├── Services/              # Service layer tests
│   ├── GraphQL/               # GraphQL tests
│   ├── Integration/           # Integration tests
│   ├── Performance/           # Load tests
│   └── Security/              # Security tests
├── docker-compose.yml         # Database and search services
├── init.sql                   # Database initialization script
├── test.sh                    # Comprehensive test runner
├── deploy.sh                  # Deployment script
└── DEPLOYMENT.md              # Production deployment guide
```

---

## 🚀 Next Steps

1. **Explore the Application**: Visit [http://localhost:3000](http://localhost:3000) to see the product search interface
2. **Try GraphQL**: Use the GraphQL playground at [http://localhost:5119/graphql](http://localhost:5119/graphql)
3. **Run Tests**: Execute the test suites to ensure everything works correctly
4. **Check Health**: Monitor application health at [http://localhost:5119/health](http://localhost:5119/health)
5. **Initialize Database**: Use `POST http://localhost:5119/api/dev/migrate-and-seed` if database is empty
6. **Production Deployment**: Follow the [DEPLOYMENT.md](DEPLOYMENT.md) guide for production setup

---

## 🤝 Development Workflow

### Making Changes

1. **Backend Changes**: The API supports hot reload, changes will be reflected immediately
2. **Frontend Changes**: Next.js with Turbopack provides instant hot reload
3. **Database Changes**: Update migration files in `backend/Database/Migrations/`
4. **Search Changes**: Re-run the sync command after database schema changes

### Testing Strategy

1. **Unit Tests**: Test individual components and services
2. **Integration Tests**: Test API endpoints and database interactions
3. **Performance Tests**: Validate system performance under load
4. **Security Tests**: Ensure application security standards

### Git Repository Management

The project uses a single repository structure with both frontend and backend code:

- All source files are properly tracked in git
- Frontend and backend are separate but integrated applications
- Database initialization scripts are included
- Comprehensive documentation and deployment scripts

---

## 📚 Additional Resources

- **Backend Documentation**: Explore GraphQL schema at [http://localhost:5119/graphql](http://localhost:5119/graphql)
- **Production Deployment**: See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions
- **Testing Guide**: Run `./test.sh` for comprehensive testing (requires bash/WSL)
- **Docker Services**: All database services are defined in `docker-compose.yml`

For issues or questions, check the troubleshooting section above or review the application logs.
