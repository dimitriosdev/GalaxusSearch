import "@testing-library/jest-dom";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter() {
    return {
      push: jest.fn(),
      replace: jest.fn(),
      prefetch: jest.fn(),
      back: jest.fn(),
      forward: jest.fn(),
      refresh: jest.fn(),
    };
  },
  useSearchParams() {
    return new URLSearchParams();
  },
  usePathname() {
    return "/";
  },
}));

// Mock environment variables
process.env.NEXT_PUBLIC_API_URL = "http://localhost:5000";
process.env.NEXT_PUBLIC_GRAPHQL_ENDPOINT = "http://localhost:5000/graphql";
process.env.NEXT_PUBLIC_ENVIRONMENT = "test";
process.env.NEXT_PUBLIC_MONITORING_ENABLED = "false";
process.env.NEXT_PUBLIC_AUTH_ENABLED = "false";

// Mock console methods to reduce noise in tests
global.console = {
  ...console,
  // Uncomment to ignore specific log levels
  // log: jest.fn(),
  // warn: jest.fn(),
  // error: jest.fn(),
};
