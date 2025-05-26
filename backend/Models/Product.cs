namespace Backend.Api.Models
{
  public class Product
  {
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
