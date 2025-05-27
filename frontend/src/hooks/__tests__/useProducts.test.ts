import { renderHook, waitFor } from "@testing-library/react";
import { MockedProvider } from "@apollo/client/testing";
import { useProducts, SEARCH_PRODUCTS } from "../useProducts";
import { ReactNode } from "react";

const mockProducts = [
  {
    id: 1,
    name: "Test Product",
    description: "Test description",
    price: 99.99,
    category: "Electronics",
    brand: "TestBrand",
    imageUrl: "https://example.com/image.jpg",
    createdAt: "2024-01-01T00:00:00Z",
    updatedAt: "2024-01-01T00:00:00Z",
  },
];

const mocks = [
  {
    request: {
      query: SEARCH_PRODUCTS,
      variables: {
        query: "laptop",
        category: null,
        minPrice: null,
        maxPrice: null,
      },
    },
    result: {
      data: {
        searchProducts: mockProducts,
      },
    },
  },
];

const errorMocks = [
  {
    request: {
      query: SEARCH_PRODUCTS,
      variables: {
        query: "error",
        category: null,
        minPrice: null,
        maxPrice: null,
      },
    },
    error: new Error("Network error"),
  },
];

const wrapper = ({ children }: { children: ReactNode }) => (
  <MockedProvider mocks={mocks} addTypename={false}>
    {children}
  </MockedProvider>
);

const errorWrapper = ({ children }: { children: ReactNode }) => (
  <MockedProvider mocks={errorMocks} addTypename={false}>
    {children}
  </MockedProvider>
);

describe("useProducts", () => {
  it("initializes with empty state", () => {
    const { result } = renderHook(() => useProducts(), { wrapper });

    expect(result.current.products).toEqual([]);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe(null);
    expect(result.current.retryCount).toBe(0);
    expect(result.current.isRetrying).toBe(false);
  });

  it("searches products successfully", async () => {
    const { result } = renderHook(() => useProducts(), { wrapper });

    await result.current.searchProducts({
      query: "laptop",
    });

    await waitFor(() => {
      expect(result.current.products).toEqual(mockProducts);
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe(null);
    });
  });

  it("handles search errors with retry logic", async () => {
    const { result } = renderHook(() => useProducts(), {
      wrapper: errorWrapper,
    });

    await result.current.searchProducts({
      query: "error",
    });

    await waitFor(() => {
      expect(result.current.error).toBeTruthy();
      expect(result.current.loading).toBe(false);
    });
  });

  it("validates search parameters", async () => {
    const { result } = renderHook(() => useProducts(), { wrapper });

    // Test empty query
    await expect(
      result.current.searchProducts({
        query: "",
      })
    ).rejects.toThrow("Search query is required");

    // Test invalid price range
    await expect(
      result.current.searchProducts({
        query: "test",
        minPrice: 100,
        maxPrice: 50,
      })
    ).rejects.toThrow("Minimum price cannot be greater than maximum price");
  });

  it("handles loading states correctly", async () => {
    const { result } = renderHook(() => useProducts(), { wrapper });

    const searchPromise = result.current.searchProducts({
      query: "laptop",
    });

    // Should be loading initially
    expect(result.current.loading).toBe(true);

    await searchPromise;

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
  });

  it("provides refetch functionality", async () => {
    const { result } = renderHook(() => useProducts(), { wrapper });

    await result.current.searchProducts({
      query: "laptop",
    });

    await waitFor(() => {
      expect(result.current.products).toEqual(mockProducts);
    });

    // Test refetch
    await result.current.refetch();

    await waitFor(() => {
      expect(result.current.products).toEqual(mockProducts);
    });
  });

  it("cancels previous requests when new search is initiated", async () => {
    const { result } = renderHook(() => useProducts(), { wrapper });

    // Start first search
    const firstSearch = result.current.searchProducts({
      query: "laptop",
    });

    // Start second search immediately
    const secondSearch = result.current.searchProducts({
      query: "laptop",
    });

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    // First search should be cancelled, only second should complete
    await expect(firstSearch).rejects.toThrow("Request cancelled");
    await expect(secondSearch).resolves.toBeUndefined();
  });
});
