# Railway Deployment

This project can be easily deployed to Railway with these steps:

## Prerequisites

1. Push your code to a GitHub repository
2. Sign up for Railway (https://railway.app)

## Deployment Steps

1. Connect Railway to your GitHub account
2. Import your repository
3. Railway will auto-detect:
   - Frontend (Next.js)
   - Backend (.NET)
   - PostgreSQL database needed

## Environment Variables

Railway will need these environment variables:

- `DATABASE_URL` (automatically provided by Railway PostgreSQL)
- `ELASTICSEARCH_URL` (if using external Elasticsearch)

## Services

Railway will create:

- Frontend service (Next.js)
- Backend service (.NET API)
- PostgreSQL database
- (Optional) Elasticsearch service

## Alternative: Local with Docker

Run locally with:

```bash
docker-compose up -d
cd backend && dotnet run
cd frontend && npm run dev
```

Access:

- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- GraphQL Playground: http://localhost:5000/graphql
