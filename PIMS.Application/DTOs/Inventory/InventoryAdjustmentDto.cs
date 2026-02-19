namespace PIMS.Application.DTOs.Inventory;

public class InventoryAdjustmentDto
{
    public Guid ProductId { get; set; }
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
}
