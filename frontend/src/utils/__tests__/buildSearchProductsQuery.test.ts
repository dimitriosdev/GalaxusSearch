import { buildSearchProductsQuery } from "../buildSearchProductsQuery";

describe("buildSearchProductsQuery", () => {
  it("builds query with all parameters", () => {
    const params = {
      query: "laptop",
      category: "Electronics",
      minPrice: 100,
      maxPrice: 500,
    };

    const result = buildSearchProductsQuery(params);

    expect(result).toEqual({
      query: "laptop",
      category: "Electronics",
      minPrice: 100,
      maxPrice: 500,
    });
  });

  it("builds query with minimal parameters", () => {
    const params = {
      query: "laptop",
    };

    const result = buildSearchProductsQuery(params);

    expect(result).toEqual({
      query: "laptop",
      category: undefined,
      minPrice: undefined,
      maxPrice: undefined,
    });
  });

  it("handles undefined parameters", () => {
    const params = {
      query: "laptop",
      category: undefined,
      minPrice: undefined,
      maxPrice: undefined,
    };

    const result = buildSearchProductsQuery(params);

    expect(result).toEqual({
      query: "laptop",
      category: undefined,
      minPrice: undefined,
      maxPrice: undefined,
    });
  });

  it("preserves null values", () => {
    const params = {
      query: "laptop",
      category: null,
      minPrice: null,
      maxPrice: null,
    };

    const result = buildSearchProductsQuery(params);

    expect(result).toEqual({
      query: "laptop",
      category: null,
      minPrice: null,
      maxPrice: null,
    });
  });

  it("handles empty strings", () => {
    const params = {
      query: "laptop",
      category: "",
      minPrice: 0,
      maxPrice: 1000,
    };

    const result = buildSearchProductsQuery(params);

    expect(result).toEqual({
      query: "laptop",
      category: "",
      minPrice: 0,
      maxPrice: 1000,
    });
  });
});
