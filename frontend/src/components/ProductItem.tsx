import React from "react";
import { Product } from "@/types/Product";

export interface ProductItemProps {
  product: Product;
  onCategoryClick: (category: string) => void;
}

export function ProductItem({ product, onCategoryClick }: ProductItemProps) {
  return (
    <li className="bg-gray-800 rounded-2xl shadow-lg p-6 flex flex-col sm:flex-row sm:items-center justify-between hover:shadow-2xl transition border border-gray-600 group">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-2 flex-wrap">
          <span className="text-lg font-semibold text-gray-100 group-hover:text-blue-300 transition-colors truncate max-w-xs">
            {product.name}
          </span>
          <span
            className="text-xs bg-blue-900 text-blue-200 px-2 py-0.5 rounded cursor-pointer hover:bg-blue-800 hover:text-blue-100 transition font-medium border border-blue-700"
            onClick={() => onCategoryClick(product.category)}
          >
            {product.category}
          </span>
        </div>
        <p className="text-gray-300 text-sm mb-1 line-clamp-2 max-w-xl truncate">
          {product.description}
        </p>
      </div>
      <div className="mt-3 sm:mt-0 sm:ml-6 flex-shrink-0 text-right">
        <span className="text-2xl font-bold text-blue-400 drop-shadow">
          ${product.price.toFixed(2)}
        </span>
      </div>
    </li>
  );
}
