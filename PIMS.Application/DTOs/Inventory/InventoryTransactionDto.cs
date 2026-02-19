namespace PIMS.Application.DTOs.Inventory;

public class InventoryTransactionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityChanged { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
