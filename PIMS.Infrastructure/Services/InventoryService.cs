using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Exceptions;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ApplicationDbContext context,
                            ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Adjust Inventory

    public async Task<object> AdjustInventoryAsync(InventoryAdjustmentDto dto, string userId)
    {
        _logger.LogInformation("Inventory adjustment started for ProductId {ProductId}", dto.ProductId);

        var product = await _context.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
        {
            _logger.LogWarning("Product not found for inventory adjustment.");
            throw new BadRequestException("Product not found.");
        }

        if (product.Inventory == null)
        {
            _logger.LogWarning("Inventory record missing for ProductId {ProductId}", dto.ProductId);
            throw new BadRequestException("Inventory record not found.");
        }

        var newQuantity = product.Inventory.Quantity + dto.QuantityChange;

        if (newQuantity < 0)
        {
            _logger.LogWarning("Insufficient stock for ProductId {ProductId}", dto.ProductId);
            throw new BadRequestException("Insufficient stock.");
        }

        product.Inventory.Quantity = newQuantity;

        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            QuantityChanged = dto.QuantityChange,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync();

        var lowStock = newQuantity <= product.LowStockThreshold;

        if (lowStock)
        {
            _logger.LogWarning("Low stock alert for ProductId {ProductId}. Quantity: {Quantity}",
                product.Id, newQuantity);
        }

        _logger.LogInformation(
            "Inventory updated for ProductId {ProductId}. Change: {Change}, NewQuantity: {NewQuantity}, By: {UserId}",
            product.Id, dto.QuantityChange, newQuantity, userId);

        return new
        {
            ProductId = product.Id,
            NewQuantity = newQuantity,
            LowStockAlert = lowStock
        };
    }

    #endregion

    #region Transaction History

    public async Task<List<InventoryTransactionDto>> GetTransactionHistoryAsync(Guid? productId)
    {
        _logger.LogInformation("Fetching inventory transaction history.");

        var query = _context.InventoryTransactions.AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new InventoryTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                QuantityChanged = t.QuantityChanged,
                Reason = t.Reason,
                CreatedAt = t.CreatedAt,
                CreatedByUserId = t.CreatedByUserId
            })
            .ToListAsync();
    }

    #endregion

    #region Low Stock

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync()
    {
        _logger.LogInformation("Fetching low stock products.");

        return await _context.Products
            .Where(p => p.Inventory != null &&
                        p.Inventory.Quantity <= p.LowStockThreshold)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                CurrentQuantity = p.Inventory!.Quantity,
                LowStockThreshold = p.LowStockThreshold
            })
            .ToListAsync();
    }

    #endregion
}