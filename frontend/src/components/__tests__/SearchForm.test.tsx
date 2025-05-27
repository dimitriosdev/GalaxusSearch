import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MockedProvider } from "@apollo/client/testing";
import SearchForm from "../SearchForm";
import { SEARCH_PRODUCTS } from "../../hooks/useProducts";

const mockSearchProducts = jest.fn();

// Mock the useProducts hook
jest.mock("../../hooks/useProducts", () => ({
  useProducts: () => ({
    products: [],
    loading: false,
    error: null,
    searchProducts: mockSearchProducts,
    retryCount: 0,
    isRetrying: false,
    refetch: jest.fn(),
  }),
  SEARCH_PRODUCTS: "SEARCH_PRODUCTS_QUERY",
}));

describe("SearchForm", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders search form elements", () => {
    render(
      <MockedProvider>
        <SearchForm />
      </MockedProvider>
    );

    expect(screen.getByPlaceholderText(/search products/i)).toBeInTheDocument();
    expect(screen.getByText(/search/i)).toBeInTheDocument();
  });

  it("calls searchProducts when form is submitted", async () => {
    const user = userEvent.setup();

    render(
      <MockedProvider>
        <SearchForm />
      </MockedProvider>
    );

    const searchInput = screen.getByPlaceholderText(/search products/i);
    const searchButton = screen.getByRole("button", { name: /search/i });

    await user.type(searchInput, "laptop");
    await user.click(searchButton);

    await waitFor(() => {
      expect(mockSearchProducts).toHaveBeenCalledWith({
        query: "laptop",
        category: undefined,
        minPrice: undefined,
        maxPrice: undefined,
      });
    });
  });

  it("handles category selection", async () => {
    const user = userEvent.setup();

    render(
      <MockedProvider>
        <SearchForm />
      </MockedProvider>
    );

    const categorySelect = screen.getByDisplayValue("All Categories");
    await user.selectOptions(categorySelect, "Electronics");

    const searchInput = screen.getByPlaceholderText(/search products/i);
    const searchButton = screen.getByRole("button", { name: /search/i });

    await user.type(searchInput, "laptop");
    await user.click(searchButton);

    await waitFor(() => {
      expect(mockSearchProducts).toHaveBeenCalledWith({
        query: "laptop",
        category: "Electronics",
        minPrice: undefined,
        maxPrice: undefined,
      });
    });
  });

  it("handles price range inputs", async () => {
    const user = userEvent.setup();

    render(
      <MockedProvider>
        <SearchForm />
      </MockedProvider>
    );

    const minPriceInput = screen.getByPlaceholderText(/min price/i);
    const maxPriceInput = screen.getByPlaceholderText(/max price/i);
    const searchInput = screen.getByPlaceholderText(/search products/i);
    const searchButton = screen.getByRole("button", { name: /search/i });

    await user.type(minPriceInput, "100");
    await user.type(maxPriceInput, "500");
    await user.type(searchInput, "laptop");
    await user.click(searchButton);

    await waitFor(() => {
      expect(mockSearchProducts).toHaveBeenCalledWith({
        query: "laptop",
        category: undefined,
        minPrice: 100,
        maxPrice: 500,
      });
    });
  });

  it("validates price range (min should not be greater than max)", async () => {
    const user = userEvent.setup();

    render(
      <MockedProvider>
        <SearchForm />
      </MockedProvider>
    );

    const minPriceInput = screen.getByPlaceholderText(/min price/i);
    const maxPriceInput = screen.getByPlaceholderText(/max price/i);
    const searchInput = screen.getByPlaceholderText(/search products/i);
    const searchButton = screen.getByRole("button", { name: /search/i });

    await user.type(minPriceInput, "500");
    await user.type(maxPriceInput, "100");
    await user.type(searchInput, "laptop");
    await user.click(searchButton);

    // Should show validation error or not call searchProducts
    expect(mockSearchProducts).not.toHaveBeenCalled();
  });

  it("prevents submission with empty search query", async () => {
    const user = userEvent.setup();

    render(
      <MockedProvider>
        <SearchForm />
      </MockedProvider>
    );

    const searchButton = screen.getByRole("button", { name: /search/i });
    await user.click(searchButton);

    expect(mockSearchProducts).not.toHaveBeenCalled();
  });
});
