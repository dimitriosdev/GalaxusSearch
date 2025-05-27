"use client";

import { useEffect, useState } from "react";
import { useProducts, SortField, SortOrder } from "@/hooks/useProducts";

import { SearchForm } from "@/components/SearchForm";
import { ProductList } from "@/components/ProductList";

export default function Home() {
  const [query, setQuery] = useState("");
  const [category, setCategory] = useState("");
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [sortField, setSortField] = useState<SortField>("name");
  const [sortOrder, setSortOrder] = useState<SortOrder>("asc");
  const [pageSize, setPageSize] = useState(20);
  const [page, setPage] = useState(1);

  const { products, loading, error, refetch, isRetrying } = useProducts({
    query,
    category,
    minPrice,
    maxPrice,
    size: 1000,
  });

  useEffect(() => {
    // Force dark mode permanently
    document.documentElement.classList.add("dark");
    document.body.style.colorScheme = "dark";
  }, []);

  // Sort products by selected field and order, or reset to name ascending
  const sortedProducts =
    sortField === "name"
      ? [...products].sort((a, b) =>
          sortOrder === "asc"
            ? a.name.localeCompare(b.name)
            : b.name.localeCompare(a.name)
        )
      : [...products].sort((a, b) =>
          sortOrder === "asc" ? a.price - b.price : b.price - a.price
        );

  // Calculate paginated products
  const totalPages = Math.ceil(sortedProducts.length / pageSize);
  const paginatedProducts = sortedProducts.slice(
    (page - 1) * pageSize,
    page * pageSize
  );

  return (
    <main className="min-h-screen bg-gradient-to-b from-gray-900 to-gray-700 flex flex-col items-center justify-center p-4">
      <div className="w-full max-w-2xl rounded-3xl bg-gray-800/95 shadow-2xl p-10 border border-gray-600">
        <h1 className="text-4xl font-extrabold mb-10 text-center tracking-tight text-blue-200 drop-shadow-lg">
          Product Search
        </h1>
        <SearchForm
          query={query}
          minPrice={minPrice}
          maxPrice={maxPrice}
          onQueryChange={setQuery}
          onMinPriceChange={setMinPrice}
          onMaxPriceChange={setMaxPrice}
        />
        {category && (
          <div className="mb-6 flex items-center gap-2 justify-center">
            <span className="text-sm bg-blue-900 text-blue-200 px-3 py-1 rounded font-medium shadow">
              Category: {category}
            </span>
            <button
              className="text-gray-500 hover:text-red-400 text-lg font-bold px-1"
              onClick={() => setCategory("")}
              aria-label="Clear category filter"
            >
              ×
            </button>
          </div>
        )}
        {loading && (
          <div className="text-center text-blue-300 mb-4 animate-pulse font-semibold">
            {isRetrying ? "Retrying..." : "Loading..."}
          </div>
        )}
        {error && (
          <div className="text-center text-red-400 mb-4 p-4 bg-red-900/20 rounded-lg border border-red-800">
            <p className="font-semibold mb-2">Error: {error}</p>
            <button
              onClick={refetch}
              className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded font-medium transition-colors"
              disabled={loading}
            >
              {loading ? "Retrying..." : "Try Again"}
            </button>
          </div>
        )}
        <div className="mb-6 flex items-center gap-4 justify-center">
          <label htmlFor="sortField" className="text-gray-300 font-medium">
            Sort by:
          </label>
          <select
            id="sortField"
            value={sortField}
            onChange={(e) => {
              setSortField(e.target.value as SortField);
              setPage(1);
            }}
            className="rounded px-2 py-1 bg-gray-700 text-gray-100 border border-gray-600 focus:outline-none"
          >
            <option value="name">Name</option>
            <option value="price">Price</option>
          </select>
          <label htmlFor="sortOrder" className="text-gray-300 font-medium">
            Order:
          </label>
          <select
            id="sortOrder"
            value={sortOrder}
            onChange={(e) => {
              setSortOrder(e.target.value as SortOrder);
              setPage(1);
            }}
            className="rounded px-2 py-1 bg-gray-700 text-gray-100 border border-gray-600 focus:outline-none"
          >
            <option value="asc">Ascending</option>
            <option value="desc">Descending</option>
          </select>
          <label htmlFor="pageSize" className="text-gray-300 font-medium">
            Items per page:
          </label>
          <select
            id="pageSize"
            value={pageSize}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPage(1);
            }}
            className="rounded px-2 py-1 bg-gray-700 text-gray-100 border border-gray-600 focus:outline-none"
          >
            <option value={20}>20</option>
            <option value={50}>50</option>
            <option value={100}>100</option>
          </select>
        </div>
        <ProductList
          products={paginatedProducts}
          onCategoryClick={setCategory}
        />
        {totalPages > 1 && (
          <div className="flex justify-center items-center gap-2 mt-6">
            <button
              className="px-3 py-1 rounded bg-gray-700 text-gray-200 border border-gray-600 hover:bg-gray-600 transition disabled:opacity-50"
              onClick={() => setPage(page - 1)}
              disabled={page === 1}
            >
              Prev
            </button>
            <span className="text-gray-300 font-medium">
              Page {page} of {totalPages}
            </span>
            <button
              className="px-3 py-1 rounded bg-gray-700 text-gray-200 border border-gray-600 hover:bg-gray-600 transition disabled:opacity-50"
              onClick={() => setPage(page + 1)}
              disabled={page === totalPages}
            >
              Next
            </button>
          </div>
        )}
        {!loading && products.length === 0 && (
          <div className="text-center text-gray-500 mt-10 text-lg font-medium">
            No products found.
          </div>
        )}
      </div>
    </main>
  );
}
