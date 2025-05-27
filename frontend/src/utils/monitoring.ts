// Monitoring and error tracking utilities
interface ErrorContext {
  userId?: string;
  sessionId?: string;
  timestamp: string;
  userAgent: string;
  url: string;
  component?: string;
  action?: string;
  metadata?: Record<string, any>;
}

interface PerformanceMetric {
  name: string;
  value: number;
  timestamp: string;
  metadata?: Record<string, any>;
}

class MonitoringService {
  private sessionId: string;
  private isProduction: boolean;

  constructor() {
    this.sessionId = this.generateSessionId();
    this.isProduction = process.env.NEXT_PUBLIC_ENVIRONMENT === "production";
  }

  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random()
      .toString(36)
      .substring(2, 15)}`;
  }

  private createErrorContext(
    error: Error,
    component?: string,
    action?: string,
    metadata?: Record<string, any>
  ): ErrorContext {
    return {
      sessionId: this.sessionId,
      timestamp: new Date().toISOString(),
      userAgent:
        typeof window !== "undefined" ? window.navigator.userAgent : "server",
      url: typeof window !== "undefined" ? window.location.href : "server",
      component,
      action,
      metadata: {
        ...metadata,
        errorName: error.name,
        errorMessage: error.message,
        errorStack: error.stack,
      },
    };
  }

  // Log errors to console in development, or send to monitoring service in production
  logError(
    error: Error,
    component?: string,
    action?: string,
    metadata?: Record<string, any>
  ): void {
    const context = this.createErrorContext(error, component, action, metadata);

    if (this.isProduction) {
      // In production, send to monitoring service (e.g., Sentry, LogRocket, etc.)
      this.sendToMonitoringService("error", context);
    } else {
      // In development, log to console with rich context
      console.group(`🚨 Error in ${component || "Unknown Component"}`);
      console.error("Error:", error);
      console.log("Context:", context);
      console.groupEnd();
    }
  }

  // Log performance metrics
  logPerformance(
    name: string,
    value: number,
    metadata?: Record<string, any>
  ): void {
    const metric: PerformanceMetric = {
      name,
      value,
      timestamp: new Date().toISOString(),
      metadata,
    };

    if (this.isProduction) {
      this.sendToMonitoringService("performance", metric);
    } else {
      console.log(`⚡ Performance - ${name}:`, `${value}ms`, metadata);
    }
  }

  // Log user actions for analytics
  logUserAction(action: string, metadata?: Record<string, any>): void {
    const event = {
      action,
      sessionId: this.sessionId,
      timestamp: new Date().toISOString(),
      metadata,
    };

    if (this.isProduction) {
      this.sendToMonitoringService("user_action", event);
    } else {
      console.log(`👤 User Action - ${action}:`, metadata);
    }
  }

  private sendToMonitoringService(type: string, data: any): void {
    // This would integrate with your monitoring service
    // For example: Sentry, LogRocket, DataDog, New Relic, etc.

    // Example implementation for a generic monitoring endpoint:
    if (typeof fetch !== "undefined") {
      fetch("/api/monitoring", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          type,
          data,
          environment: process.env.NEXT_PUBLIC_ENVIRONMENT,
        }),
      }).catch((err) => {
        // Fallback: log to console if monitoring service fails
        console.warn("Failed to send to monitoring service:", err);
        console.log("Original data:", { type, data });
      });
    }
  }
}

// Singleton instance
export const monitoring = new MonitoringService();

// Performance measurement utility
export function measurePerformance<T>(
  name: string,
  fn: () => T | Promise<T>,
  metadata?: Record<string, any>
): T | Promise<T> {
  const start = performance.now();

  try {
    const result = fn();

    if (result instanceof Promise) {
      return result
        .then((value) => {
          const duration = performance.now() - start;
          monitoring.logPerformance(name, duration, metadata);
          return value;
        })
        .catch((error) => {
          const duration = performance.now() - start;
          monitoring.logPerformance(`${name}_error`, duration, metadata);
          throw error;
        });
    } else {
      const duration = performance.now() - start;
      monitoring.logPerformance(name, duration, metadata);
      return result;
    }
  } catch (error) {
    const duration = performance.now() - start;
    monitoring.logPerformance(`${name}_error`, duration, metadata);
    throw error;
  }
}

// Error boundary helper
export function withErrorBoundary<T extends (...args: any[]) => any>(
  fn: T,
  component: string
): T {
  return ((...args: any[]) => {
    try {
      return fn(...args);
    } catch (error) {
      monitoring.logError(error as Error, component, "function_call", { args });
      throw error;
    }
  }) as T;
}

// React hook for monitoring
export function useMonitoring(componentName: string) {
  const logError = (
    error: Error,
    action?: string,
    metadata?: Record<string, any>
  ) => {
    monitoring.logError(error, componentName, action, metadata);
  };

  const logUserAction = (action: string, metadata?: Record<string, any>) => {
    monitoring.logUserAction(action, { ...metadata, component: componentName });
  };

  const logPerformance = (
    name: string,
    value: number,
    metadata?: Record<string, any>
  ) => {
    monitoring.logPerformance(`${componentName}_${name}`, value, metadata);
  };

  return {
    logError,
    logUserAction,
    logPerformance,
  };
}
