interface BuildSearchProductsQueryArgs {
  query?: string;
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  size?: number;
}

export function buildSearchProductsQuery({
  query,
  category,
  minPrice,
  maxPrice,
  size = 20, // Changed from 1000 to 20 to match backend default
}: BuildSearchProductsQueryArgs): string {
  const filters = [];
  
  // Only add non-empty/valid parameters
  if (query && query.trim()) {
    filters.push(`query: "${query.trim()}"`);
  }
  if (category && category.trim()) {
    filters.push(`category: "${category.trim()}"`);
  }
  if (minPrice !== undefined && minPrice >= 0) {
    filters.push(`minPrice: ${minPrice}`);
  }
  if (maxPrice !== undefined && maxPrice >= 0) {
    filters.push(`maxPrice: ${maxPrice}`);
  }
  // Always include size parameter, ensure it's within valid range
  const validSize = Math.max(1, Math.min(1000, size));
  filters.push(`size: ${validSize}`);
  
  return `query {
  searchProducts(${filters.join(", ")}) {
    id
    name
    description
    price
    category
  }
}`;
}
