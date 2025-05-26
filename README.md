# Galaxus Project: Developer Setup Guide

This guide will help you get the full stack (frontend, backend, PostgreSQL, and Elasticsearch) running locally from scratch.

---

## 1. Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) installed
- [Node.js](https://nodejs.org/) (v18+) and npm installed

---

## 2. Start Database & Search Services

From the project root (`galaxus/`), run:

```powershell
docker compose up -d
```

This will start:

- **PostgreSQL** (port 5432, user: `galaxus`, password: `galaxus`, db: `galaxus`)
- **Elasticsearch** (port 9200)

The database will be automatically seeded with 10,000+ products.

---

## 3. Start the Backend API (.NET)

In a new terminal:

```powershell
cd backend
# Restore dependencies
 dotnet restore
# Run the API
 dotnet run
```

The backend will start on [http://localhost:5000](http://localhost:5000) (or as configured).

---

## 4. Start the Frontend (Next.js)

In another terminal:

```powershell
cd frontend
npm install
npm run dev
```

The frontend will start on [http://localhost:3000](http://localhost:3000).

---

## 5. Sync Products to Elasticsearch (for search)

After the backend is running, trigger a sync from PostgreSQL to Elasticsearch:

```powershell
curl -X POST http://localhost:5000/sync-elastic
```

This will index all products from the database into Elasticsearch for search functionality.

---

## 6. Stopping Services

To stop the database and Elasticsearch:

```powershell
docker compose down
```

---

## 7. Useful URLs

- Frontend: [http://localhost:3000](http://localhost:3000)
- Backend API: [http://localhost:5000](http://localhost:5000)
- PostgreSQL: `localhost:5432` (user: `galaxus`, password: `galaxus`)
- Elasticsearch: [http://localhost:9200](http://localhost:9200)

---

## 8. Troubleshooting

- Make sure Docker Desktop is running before starting services.
- If ports are in use, stop other containers or change the ports in `docker-compose.yml`.
- For database connection errors, check credentials in your backend config.
- If search does not work, ensure you have run the sync step above.

---

## 9. Project Structure

- `backend/` - .NET API
- `frontend/` - Next.js app
- `docker-compose.yml` - Service definitions for PostgreSQL and Elasticsearch
- `init.sql` - Database seed script
