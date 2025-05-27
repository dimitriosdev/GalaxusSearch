import { monitoring } from "../utils/monitoring";

// Mock monitoring functions for tests
export const mockMonitoring = {
  trackError: jest.fn(),
  trackPerformance: jest.fn(),
  trackUserAction: jest.fn(),
  setUser: jest.fn(),
  addBreadcrumb: jest.fn(),
};

// Replace real monitoring with mocks in test environment
if (process.env.NODE_ENV === "test") {
  Object.assign(monitoring, mockMonitoring);
}

export default mockMonitoring;
