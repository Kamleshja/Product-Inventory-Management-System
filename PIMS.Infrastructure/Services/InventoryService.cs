using Microsoft.EntityFrameworkCore;
using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Exceptions;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> AdjustInventoryAsync(InventoryAdjustmentDto dto, string userId)
    {
        var product = await _context.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
            throw new BadRequestException("Product not found.");

        if (product.Inventory == null)
            throw new BadRequestException("Inventory record not found.");

        var newQuantity = product.Inventory.Quantity + dto.QuantityChange;

        if (newQuantity < 0)
            throw new BadRequestException("Insufficient stock.");

        // Update Inventory
        product.Inventory.Quantity = newQuantity;

        // Record Transaction
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

        return new
        {
            ProductId = product.Id,
            NewQuantity = newQuantity,
            LowStockAlert = lowStock
        };
    }
    public async Task<List<InventoryTransactionDto>> GetTransactionHistoryAsync(Guid? productId)
    {
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
    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync()
    {
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
}
