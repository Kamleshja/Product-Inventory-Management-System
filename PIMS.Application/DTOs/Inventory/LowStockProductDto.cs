namespace PIMS.Application.DTOs.Inventory;

public class LowStockProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int LowStockThreshold { get; set; }
}
