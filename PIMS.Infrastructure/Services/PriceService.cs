using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PIMS.Application.DTOs.Product;
using PIMS.Application.Exceptions;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Services;

public class PriceService : IPriceService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PriceService> _logger;

    private const string ProductsCacheKey = "products_all";

    public PriceService(ApplicationDbContext context,
                        IMemoryCache cache,
                        ILogger<PriceService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    #region Individual Price Update

    public async Task UpdatePriceAsync(UpdateProductPriceDto request, string userId)
    {
        _logger.LogInformation("Updating price for ProductId {ProductId}", request.ProductId);

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
        {
            _logger.LogWarning("Product not found for ID {ProductId}", request.ProductId);
            throw new BadRequestException("Product not found.");
        }

        var roundedPrice = Math.Round(request.NewPrice, 2);

        if (product.Price == roundedPrice)
        {
            _logger.LogInformation("Price unchanged for ProductId {ProductId}", product.Id);
            return;
        }

        var oldPrice = product.Price;

        product.Price = roundedPrice;

        var history = new ProductPriceHistory
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            OldPrice = oldPrice,
            NewPrice = roundedPrice,
            Reason = request.Reason,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        };

        _context.ProductPriceHistories.Add(history);

        await _context.SaveChangesAsync();

        // Invalidate cache
        _cache.Remove(ProductsCacheKey);

        _logger.LogInformation(
            "Price updated successfully for ProductId {ProductId}. Old: {OldPrice}, New: {NewPrice}, By: {UserId}",
            product.Id, oldPrice, roundedPrice, userId);
    }

    #endregion

    #region Bulk Price Update

    public async Task BulkUpdatePriceAsync(BulkPriceUpdateDto request, string userId)
    {
        _logger.LogInformation("Bulk price update started by User {UserId}", userId);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var products = await _context.Products
                .Where(p => request.ProductIds.Contains(p.Id))
                .ToListAsync();

            if (!products.Any())
            {
                _logger.LogWarning("Bulk update failed. No valid products found.");
                throw new BadRequestException("No valid products found.");
            }

            foreach (var product in products)
            {
                var oldPrice = product.Price;

                decimal newPrice = request.AdjustmentType == "Percentage"
                    ? oldPrice - (oldPrice * request.Value / 100)
                    : oldPrice - request.Value;

                newPrice = Math.Round(newPrice, 2);

                if (newPrice < 0)
                {
                    _logger.LogWarning(
                        "Bulk update aborted. Negative price detected for ProductId {ProductId}",
                        product.Id);

                    throw new BadRequestException(
                        $"Adjustment would make price negative for product {product.Name}");
                }

                product.Price = newPrice;

                var history = new ProductPriceHistory
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    OldPrice = oldPrice,
                    NewPrice = newPrice,
                    Reason = request.Reason,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow
                };

                _context.ProductPriceHistories.Add(history);

                _logger.LogInformation(
                    "Price updated for ProductId {ProductId}. Old: {OldPrice}, New: {NewPrice}",
                    product.Id, oldPrice, newPrice);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Invalidate cache
            _cache.Remove(ProductsCacheKey);

            _logger.LogInformation("Bulk price update completed successfully by User {UserId}", userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            _logger.LogError(ex, "Bulk price update failed. Transaction rolled back.");

            throw;
        }
    }

    #endregion
}