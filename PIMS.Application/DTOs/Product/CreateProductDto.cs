namespace PIMS.Application.DTOs.Product;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int LowStockThreshold { get; set; }
    public List<Guid> CategoryIds { get; set; } = new();
}