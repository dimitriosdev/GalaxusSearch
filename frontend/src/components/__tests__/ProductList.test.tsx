import { render, screen } from "@testing-library/react";
import ProductList from "../ProductList";
import { Product } from "../../types/Product";

const mockProducts: Product[] = [
  {
    id: 1,
    name: "Test Product 1",
    description: "Test description 1",
    price: 99.99,
    category: "Electronics",
    brand: "TestBrand",
    imageUrl: "https://example.com/image1.jpg",
    createdAt: "2024-01-01T00:00:00Z",
    updatedAt: "2024-01-01T00:00:00Z",
  },
  {
    id: 2,
    name: "Test Product 2",
    description: "Test description 2",
    price: 149.99,
    category: "Books",
    brand: "AnotherBrand",
    imageUrl: "https://example.com/image2.jpg",
    createdAt: "2024-01-02T00:00:00Z",
    updatedAt: "2024-01-02T00:00:00Z",
  },
];

describe("ProductList", () => {
  it("renders list of products", () => {
    render(<ProductList products={mockProducts} />);

    expect(screen.getByText("Test Product 1")).toBeInTheDocument();
    expect(screen.getByText("Test Product 2")).toBeInTheDocument();
    expect(screen.getByText("$99.99")).toBeInTheDocument();
    expect(screen.getByText("$149.99")).toBeInTheDocument();
  });

  it("renders empty state when no products", () => {
    render(<ProductList products={[]} />);

    expect(screen.getByText(/no products found/i)).toBeInTheDocument();
  });

  it("displays product categories", () => {
    render(<ProductList products={mockProducts} />);

    expect(screen.getByText("Electronics")).toBeInTheDocument();
    expect(screen.getByText("Books")).toBeInTheDocument();
  });

  it("displays product brands", () => {
    render(<ProductList products={mockProducts} />);

    expect(screen.getByText("TestBrand")).toBeInTheDocument();
    expect(screen.getByText("AnotherBrand")).toBeInTheDocument();
  });

  it("renders product images with correct alt text", () => {
    render(<ProductList products={mockProducts} />);

    const image1 = screen.getByAltText("Test Product 1");
    const image2 = screen.getByAltText("Test Product 2");

    expect(image1).toBeInTheDocument();
    expect(image2).toBeInTheDocument();
    expect(image1).toHaveAttribute("src", "https://example.com/image1.jpg");
    expect(image2).toHaveAttribute("src", "https://example.com/image2.jpg");
  });

  it("handles missing optional product properties gracefully", () => {
    const incompleteProduct: Product = {
      id: 3,
      name: "Incomplete Product",
      description: "Test description",
      price: 49.99,
      category: "Test",
      brand: "",
      imageUrl: "",
      createdAt: "2024-01-03T00:00:00Z",
      updatedAt: "2024-01-03T00:00:00Z",
    };

    render(<ProductList products={[incompleteProduct]} />);

    expect(screen.getByText("Incomplete Product")).toBeInTheDocument();
    expect(screen.getByText("$49.99")).toBeInTheDocument();
  });
});
