This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.

# Galaxus Frontend: Developer Quickstart

This project is the Next.js frontend for the Galaxus stack. To run the full stack locally (frontend, backend, PostgreSQL, Elasticsearch):

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

This starts:

- **PostgreSQL** (port 5432, user: `galaxus`, password: `galaxus`, db: `galaxus`)
- **Elasticsearch** (port 9200)

---

## 3. Backend API (.NET)

In a new terminal:

```powershell
cd backend
 dotnet restore
 dotnet run
```

Backend runs on [http://localhost:5000](http://localhost:5000) (or as configured).

---

## 4. Frontend (Next.js)

In another terminal:

```powershell
cd frontend
npm install
npm run dev
```

Frontend runs on [http://localhost:3000](http://localhost:3000).

---

## 5. Stopping Services

To stop database and Elasticsearch:

```powershell
docker compose down
```

---

## 6. Useful URLs

- Frontend: [http://localhost:3000](http://localhost:3000)
- Backend API: [http://localhost:5000](http://localhost:5000)
- PostgreSQL: `localhost:5432` (user: `galaxus`, password: `galaxus`)
- Elasticsearch: [http://localhost:9200](http://localhost:9200)

---

## 7. Troubleshooting

- Ensure Docker Desktop is running before starting services.
- If ports are in use, stop other containers or change ports in `docker-compose.yml`.
- For database connection errors, check credentials in your backend config.

---

## 8. Project Structure

- `backend/` - .NET API
- `frontend/` - Next.js app
- `docker-compose.yml` - PostgreSQL & Elasticsearch services

---

# (Original Next.js README below)

This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
