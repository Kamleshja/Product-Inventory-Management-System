using PIMS.Application.DTOs.Product;

namespace PIMS.Application.Interfaces;

public interface IPriceService
{
    Task UpdatePriceAsync(UpdateProductPriceDto request, string userId);
    Task BulkUpdatePriceAsync(BulkPriceUpdateDto request, string userId);
}