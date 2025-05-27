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
  size = 1000,
}: BuildSearchProductsQueryArgs): string {
  const filters = [];
  if (query) filters.push(`query: "${query}"`);
  if (category) filters.push(`category: "${category}"`);
  if (minPrice !== undefined) filters.push(`minPrice: ${minPrice}`);
  if (maxPrice !== undefined) filters.push(`maxPrice: ${maxPrice}`);
  if (size !== undefined) filters.push(`size: ${size}`);
  return `query {\n  searchProducts(${filters.join(
    ", "
  )}) {\n    id\n    name\n    description\n    price\n    category\n  }\n}`;
}
