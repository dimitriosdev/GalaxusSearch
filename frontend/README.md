# Galaxus Frontend

This is the Next.js frontend for the Galaxus Product Search application, built with React 19, TypeScript, and Apollo Client for GraphQL integration.

## Quick Start

For the complete setup guide including backend and database services, see the main [README.md](../README.md) in the project root.

### Frontend Only Setup

If the backend and database services are already running:

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at [http://localhost:3000](http://localhost:3000).

## Features

- **Real-time Product Search**: Debounced search with GraphQL integration
- **Responsive Design**: Mobile-friendly interface with Tailwind CSS
- **Dark/Light Mode**: Theme toggle with system preference detection
- **Error Handling**: Comprehensive error boundaries and user feedback
- **Performance Optimized**: Built with Next.js 15 and Turbopack

## Configuration

### Environment Variables

Create `.env.local` for development:

```env
NEXT_PUBLIC_API_URL=http://localhost:5119
NEXT_PUBLIC_ENVIRONMENT=development
NEXT_PUBLIC_API_TIMEOUT=30000
NEXT_PUBLIC_RETRY_ATTEMPTS=3
NEXT_PUBLIC_RETRY_DELAY=1000
```

### Backend Integration

The frontend connects to the .NET GraphQL backend running on port 5119. Ensure the backend is running before starting the frontend.

## Development

### Available Scripts

```bash
# Development server with hot reload
npm run dev

# Production build
npm run build

# Start production server
npm start

# Run tests
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage

# Linting
npm run lint
```

### Project Structure

```
src/
├── app/                    # Next.js App Router pages
├── components/             # React components
│   ├── ProductItem.tsx     # Individual product display
│   ├── ProductList.tsx     # Product grid/list
│   ├── SearchForm.tsx      # Search input with filters
│   └── ThemeToggle.tsx     # Dark/light mode toggle
├── hooks/                  # Custom React hooks
│   └── useProducts.ts      # Product search and state management
├── types/                  # TypeScript type definitions
└── utils/                  # Utility functions and monitoring
```

## Testing

The project includes comprehensive test coverage:

- Component tests with React Testing Library
- Hook tests for custom React hooks
- Integration tests for GraphQL queries
- Accessibility tests

## Deployment

For production deployment instructions, see [DEPLOYMENT.md](../DEPLOYMENT.md).

### Vercel Deployment

The easiest way to deploy is using [Vercel](https://vercel.com/new):

1. Connect your GitHub repository
2. Configure environment variables
3. Deploy automatically on push

---

## Next.js Information

This project was bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app) and uses:

- **Next.js 15** with App Router
- **React 19** with TypeScript
- **Tailwind CSS** for styling
- **Apollo Client** for GraphQL
- **Jest** for testing

### Learn More

- [Next.js Documentation](https://nextjs.org/docs)
- [Apollo Client Documentation](https://www.apollographql.com/docs/react/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
