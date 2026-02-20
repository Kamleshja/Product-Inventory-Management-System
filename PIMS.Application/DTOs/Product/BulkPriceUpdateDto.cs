namespace PIMS.Application.DTOs.Product;

public class BulkPriceUpdateDto
{
    public List<Guid> ProductIds { get; set; } = new();

    // "Percentage" or "Fixed"
    public string AdjustmentType { get; set; } = null!;
    public decimal Value { get; set; }
    public string Reason { get; set; } = null!;
}