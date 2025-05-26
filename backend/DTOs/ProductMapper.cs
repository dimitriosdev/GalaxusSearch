using Backend.Api.Models;
using Backend.Api.DTOs;

namespace Backend.Api.DTOs
{
  public static class ProductMapper
  {
    public static ProductDto ToDto(Product product)
    {
      return new ProductDto
      {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Category = product.Category,
        Brand = product.Brand,
        Sku = product.Sku,
        Stock = product.Stock,
        CreatedAt = product.CreatedAt
      };
    }

    public static Product ToModel(ProductDto dto)
    {
      return new Product
      {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Category = dto.Category,
        Brand = dto.Brand,
        Sku = dto.Sku,
        Stock = dto.Stock,
        CreatedAt = dto.CreatedAt
      };
    }
  }
}
