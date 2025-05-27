import React from "react";
import { Product } from "@/types/Product";
import { ProductItem } from "./ProductItem";

export interface ProductListProps {
  products: Product[];
  onCategoryClick: (category: string) => void;
}

export function ProductList({ products, onCategoryClick }: ProductListProps) {
  return (
    <ul className="space-y-6">
      {products.map((product) => (
        <ProductItem
          key={product.id}
          product={product}
          onCategoryClick={onCategoryClick}
        />
      ))}
    </ul>
  );
}
