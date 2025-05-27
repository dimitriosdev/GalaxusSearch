import { useCallback, useEffect, useState, useRef } from "react";
import { buildSearchProductsQuery } from "@/utils/buildSearchProductsQuery";
import { Product } from "@/types/Product";
import { useMonitoring, measurePerformance } from "@/utils/monitoring";

export type SortField = "price" | "name";
export type SortOrder = "asc" | "desc";

interface UseProductsConfig {
  query: string;
  category: string;
  minPrice?: string;
  maxPrice?: string;
  size?: number;
  enableRetry?: boolean;
  retryAttempts?: number;
  retryDelay?: number;
}

interface UseProductsResult {
  products: Product[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  isRetrying: boolean;
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5119";
const DEFAULT_RETRY_ATTEMPTS = 3;
const DEFAULT_RETRY_DELAY = 1000; // 1 second

export function useProducts({
  query,
  category,
  minPrice,
  maxPrice,
  size = 1000,
  enableRetry = true,
  retryAttempts = DEFAULT_RETRY_ATTEMPTS,
  retryDelay = DEFAULT_RETRY_DELAY,
}: UseProductsConfig): UseProductsResult {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isRetrying, setIsRetrying] = useState(false);

  // Monitoring - use refs to avoid dependency issues
  const { logError, logUserAction, logPerformance } =
    useMonitoring("useProducts");
  const logErrorRef = useRef(logError);
  const logUserActionRef = useRef(logUserAction);
  const logPerformanceRef = useRef(logPerformance);

  // Update refs when monitoring functions change
  useEffect(() => {
    logErrorRef.current = logError;
    logUserActionRef.current = logUserAction;
    logPerformanceRef.current = logPerformance;
  }, [logError, logUserAction, logPerformance]);

  // Use ref to track if component is mounted to avoid state updates on unmounted component
  const isMountedRef = useRef(true);
  const abortControllerRef = useRef<AbortController | null>(null);

  useEffect(() => {
    isMountedRef.current = true;
    return () => {
      isMountedRef.current = false;
      // Cancel any ongoing requests
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

  const fetchWithRetry = useCallback(
    async (
      url: string,
      options: RequestInit,
      attempts: number = 0
    ): Promise<Response> => {
      try {
        const response = await fetch(url, options);

        // Handle HTTP errors
        if (!response.ok) {
          if (
            response.status >= 500 &&
            attempts < retryAttempts &&
            enableRetry
          ) {
            // Server error, retry
            throw new Error(`Server error: ${response.status}`);
          } else if (response.status >= 400) {
            // Client error, don't retry
            const errorText = await response.text();
            throw new Error(
              `Request failed: ${response.status} - ${errorText}`
            );
          }
        }

        return response;
      } catch (err) {
        if (
          attempts < retryAttempts &&
          enableRetry &&
          !(err instanceof DOMException && err.name === "AbortError")
        ) {
          if (isMountedRef.current) {
            setIsRetrying(true);
          }

          // Exponential backoff
          const delay = retryDelay * Math.pow(2, attempts);
          await new Promise((resolve) => setTimeout(resolve, delay));

          if (isMountedRef.current) {
            setIsRetrying(false);
          }

          return fetchWithRetry(url, options, attempts + 1);
        }
        throw err;
      }
    },
    [enableRetry, retryAttempts, retryDelay]
  );

  const fetchProducts = useCallback(async () => {
    // Cancel any previous request
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    // Create new abort controller for this request
    abortControllerRef.current = new AbortController();

    if (!isMountedRef.current) return;

    setLoading(true);
    setError(null);
    setIsRetrying(false);

    const startTime = performance.now();

    try {
      // Input validation
      if (minPrice && parseFloat(minPrice) < 0) {
        throw new Error("Minimum price cannot be negative");
      }
      if (maxPrice && parseFloat(maxPrice) < 0) {
        throw new Error("Maximum price cannot be negative");
      }
      if (minPrice && maxPrice && parseFloat(minPrice) > parseFloat(maxPrice)) {
        throw new Error("Minimum price cannot be greater than maximum price");
      }
      if (size <= 0 || size > 1000) {
        throw new Error("Size must be between 1 and 1000");
      }

      const queryString = buildSearchProductsQuery({
        query: query.trim() || undefined,
        category: category || undefined,
        minPrice: minPrice ? parseFloat(minPrice) : undefined,
        maxPrice: maxPrice ? parseFloat(maxPrice) : undefined,
        size,
      });

      const response = await measurePerformance(
        "product_search_api_call",
        () =>
          fetchWithRetry(`${API_BASE_URL}/graphql`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Accept: "application/json",
            },
            body: JSON.stringify({ query: queryString }),
            signal: abortControllerRef.current!.signal,
          }),
        { query, category, minPrice, maxPrice, size }
      );

      const json = await response.json();

      if (json.errors && json.errors.length > 0) {
        const errorMessage = json.errors[0].message || "GraphQL query failed";
        throw new Error(errorMessage);
      }

      if (!json.data || !json.data.searchProducts) {
        throw new Error("Invalid response format");
      }

      if (isMountedRef.current) {
        setProducts(json.data.searchProducts);
        const duration = performance.now() - startTime;
        logPerformanceRef.current("fetch_products_success", duration, {
          productCount: json.data.searchProducts.length,
          query,
          category,
          minPrice,
          maxPrice,
          size,
        });

        logUserActionRef.current("search_products", {
          query,
          category,
          minPrice,
          maxPrice,
          size,
          resultCount: json.data.searchProducts.length,
        });
      }
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === "AbortError") {
        // Request was cancelled, don't set error
        return;
      }

      if (isMountedRef.current) {
        const errorMessage =
          err instanceof Error ? err.message : "An unexpected error occurred";
        setError(errorMessage);
        setProducts([]);

        const duration = performance.now() - startTime;
        logErrorRef.current(err as Error, "fetch_products", {
          query,
          category,
          minPrice,
          maxPrice,
          size,
          duration,
        });
      }
    } finally {
      if (isMountedRef.current) {
        setLoading(false);
        setIsRetrying(false);
      }
    }
  }, [
    query,
    category,
    minPrice,
    maxPrice,
    size,
    fetchWithRetry,
    // Removed monitoring functions from dependencies to prevent infinite loop
  ]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const refetch = useCallback(async () => {
    await fetchProducts();
  }, [fetchProducts]);

  return {
    products,
    loading,
    error,
    refetch,
    isRetrying,
  };
}
