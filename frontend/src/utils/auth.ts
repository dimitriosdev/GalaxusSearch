// Authentication and authorization utilities
export interface User {
  id: string;
  email: string;
  name: string;
  roles: string[];
  permissions: string[];
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
  loading: boolean;
  error: string | null;
}

export interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  refreshToken: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}

// JWT token utilities
export class TokenService {
  private static readonly TOKEN_KEY = "auth_token";
  private static readonly REFRESH_TOKEN_KEY = "refresh_token";

  static getToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(this.TOKEN_KEY);
  }

  static setToken(token: string): void {
    if (typeof window === "undefined") return;
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  static getRefreshToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  static setRefreshToken(token: string): void {
    if (typeof window === "undefined") return;
    localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
  }

  static removeTokens(): void {
    if (typeof window === "undefined") return;
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
  }

  static isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      const currentTime = Date.now() / 1000;
      return payload.exp < currentTime;
    } catch {
      return true;
    }
  }

  static decodeToken(token: string): any {
    try {
      return JSON.parse(atob(token.split(".")[1]));
    } catch {
      return null;
    }
  }
}

// Role-based access control
export class RBACService {
  static hasPermission(user: User | null, permission: string): boolean {
    if (!user) return false;
    return (
      user.permissions.includes(permission) || user.roles.includes("admin")
    );
  }

  static hasRole(user: User | null, role: string): boolean {
    if (!user) return false;
    return user.roles.includes(role);
  }

  static hasAnyRole(user: User | null, roles: string[]): boolean {
    if (!user) return false;
    return roles.some((role) => user.roles.includes(role));
  }

  static hasAllRoles(user: User | null, roles: string[]): boolean {
    if (!user) return false;
    return roles.every((role) => user.roles.includes(role));
  }
}

// API service with authentication
export class AuthenticatedApiService {
  private baseUrl: string;

  constructor(
    baseUrl: string = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5119"
  ) {
    this.baseUrl = baseUrl;
  }

  private async makeRequest(
    endpoint: string,
    options: RequestInit = {},
    requireAuth: boolean = true
  ): Promise<Response> {
    const token = TokenService.getToken();

    if (requireAuth && !token) {
      throw new Error("Authentication required");
    }

    const headers: HeadersInit = {
      "Content-Type": "application/json",
      ...options.headers,
    };

    if (token && requireAuth) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers,
    });

    if (response.status === 401 && requireAuth) {
      // Token expired, try to refresh
      try {
        await this.refreshToken();
        const newToken = TokenService.getToken();
        if (newToken) {
          headers.Authorization = `Bearer ${newToken}`;
          return fetch(`${this.baseUrl}${endpoint}`, {
            ...options,
            headers,
          });
        }
      } catch {
        // Refresh failed, redirect to login
        TokenService.removeTokens();
        window.location.href = "/login";
        throw new Error("Session expired");
      }
    }

    return response;
  }

  async login(
    email: string,
    password: string
  ): Promise<{ user: User; token: string; refreshToken: string }> {
    const response = await this.makeRequest(
      "/auth/login",
      {
        method: "POST",
        body: JSON.stringify({ email, password }),
      },
      false
    );

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || "Login failed");
    }

    return response.json();
  }

  async refreshToken(): Promise<{ token: string; refreshToken: string }> {
    const refreshToken = TokenService.getRefreshToken();
    if (!refreshToken) {
      throw new Error("No refresh token available");
    }

    const response = await this.makeRequest(
      "/auth/refresh",
      {
        method: "POST",
        body: JSON.stringify({ refreshToken }),
      },
      false
    );

    if (!response.ok) {
      throw new Error("Token refresh failed");
    }

    const data = await response.json();
    TokenService.setToken(data.token);
    TokenService.setRefreshToken(data.refreshToken);

    return data;
  }

  async getCurrentUser(): Promise<User> {
    const response = await this.makeRequest("/auth/me");

    if (!response.ok) {
      throw new Error("Failed to get current user");
    }

    return response.json();
  }

  async logout(): Promise<void> {
    try {
      await this.makeRequest("/auth/logout", { method: "POST" });
    } catch {
      // Ignore logout errors, just clear local tokens
    } finally {
      TokenService.removeTokens();
    }
  }
}

// Production-ready GraphQL client with authentication
export class AuthenticatedGraphQLClient {
  private apiService: AuthenticatedApiService;

  constructor(baseUrl?: string) {
    this.apiService = new AuthenticatedApiService(baseUrl);
  }

  async query<T = any>(
    query: string,
    variables?: Record<string, any>,
    requireAuth: boolean = false
  ): Promise<T> {
    const response = await this.apiService["makeRequest"](
      "/graphql",
      {
        method: "POST",
        body: JSON.stringify({
          query,
          variables,
        }),
      },
      requireAuth
    );

    const data = await response.json();

    if (data.errors && data.errors.length > 0) {
      throw new Error(data.errors[0].message);
    }

    return data.data;
  }
}

// Default instances
export const apiService = new AuthenticatedApiService();
export const graphqlClient = new AuthenticatedGraphQLClient();
