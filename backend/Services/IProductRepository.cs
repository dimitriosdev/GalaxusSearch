using Backend.Api.Models;
using System.Collections.Generic;

namespace Backend.Api.Services
{
  public interface IProductRepository
  {
    IEnumerable<Product> GetAllProducts();
    Product? GetProductById(string id);
    // Add more methods as needed (Add, Update, Delete)
  }
}
