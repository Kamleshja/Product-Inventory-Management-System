using PIMS.Application.DTOs.Inventory;

namespace PIMS.Application.Interfaces;

public interface IInventoryService
{
    Task<object> AdjustInventoryAsync(InventoryAdjustmentDto dto, string userId);
    Task<List<InventoryTransactionDto>> GetTransactionHistoryAsync(Guid? productId);
    Task<List<LowStockProductDto>> GetLowStockProductsAsync();
}