import React from "react";

export interface SearchFormProps {
  query: string;
  minPrice: string;
  maxPrice: string;
  onQueryChange: (v: string) => void;
  onMinPriceChange: (v: string) => void;
  onMaxPriceChange: (v: string) => void;
}

export function SearchForm({
  query,
  minPrice,
  maxPrice,
  onQueryChange,
  onMinPriceChange,
  onMaxPriceChange,
}: SearchFormProps) {
  return (
    <form
      className="flex flex-col sm:flex-row gap-4 mb-10"
      onSubmit={(e) => e.preventDefault()}
    >
      <div className="relative flex-1">
        <input
          type="text"
          placeholder="Search products..."
          className="w-full rounded-xl border-2 border-gray-600 px-5 py-4 text-base font-medium placeholder-gray-500 bg-gray-700 text-gray-100 focus:outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-900 transition-all duration-200 ease-in-out hover:border-gray-500 hover:shadow-md shadow-sm"
          value={query}
          onChange={(e) => onQueryChange(e.target.value)}
          minLength={3}
          autoFocus
        />
        <div className="absolute inset-y-0 right-0 flex items-center pr-4 pointer-events-none">
          <svg
            className="w-5 h-5 text-gray-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
        </div>
      </div>
      <div className="relative">
        <input
          type="number"
          placeholder="Min Price"
          className="w-36 rounded-xl border-2 border-green-700 px-4 py-4 text-base font-medium placeholder-gray-500 bg-gray-700 text-gray-100 focus:outline-none focus:border-green-500 focus:ring-4 focus:ring-green-900 transition-all duration-200 ease-in-out hover:border-green-600 hover:shadow-md shadow-sm"
          value={minPrice}
          onChange={(e) => onMinPriceChange(e.target.value)}
          min={0}
        />
      </div>
      <div className="relative">
        <input
          type="number"
          placeholder="Max Price"
          className="w-36 rounded-xl border-2 border-red-700 px-4 py-4 text-base font-medium placeholder-gray-500 bg-gray-700 text-gray-100 focus:outline-none focus:border-red-500 focus:ring-4 focus:ring-red-900 transition-all duration-200 ease-in-out hover:border-red-600 hover:shadow-md shadow-sm"
          value={maxPrice}
          onChange={(e) => onMaxPriceChange(e.target.value)}
          min={0}
        />
      </div>
    </form>
  );
}
