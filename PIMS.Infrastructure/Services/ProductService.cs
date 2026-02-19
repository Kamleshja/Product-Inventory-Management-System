using Microsoft.EntityFrameworkCore;
using PIMS.Application.DTOs.Product;
using PIMS.Application.Exceptions;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        // Check SKU uniqueness
        if (await _context.Products.AnyAsync(p => p.SKU == dto.SKU))
        {
            throw new BadRequestException("SKU already exists.");
        }

        // Validate categories exist
        var categories = await _context.Categories
            .Where(c => dto.CategoryIds.Contains(c.Id))
            .ToListAsync();

        if (categories.Count != dto.CategoryIds.Count)
        {
            throw new BadRequestException("One or more categories not found.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            LowStockThreshold = dto.LowStockThreshold
        };

        foreach (var category in categories)
        {
            product.ProductCategories.Add(new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = category.Id
            });
        }

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = 0,
            WarehouseLocation = "Default"
        };

        product.Inventory = inventory;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Price = product.Price,
            Quantity = 0
        };
    }
    public async Task<List<ProductResponseDto>> GetAllProductsAsync()
    {
        return await _context.Products
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                Quantity = p.Inventory != null ? p.Inventory.Quantity : 0
            })
            .ToListAsync();
    }
    public async Task<List<ProductResponseDto>> GetProductsAsync(ProductQueryDto query)
    {
        var productsQuery = _context.Products
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .AsQueryable();

        // Filter by Category
        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery
                .Where(p => p.ProductCategories
                    .Any(pc => pc.CategoryId == query.CategoryId));
        }

        // Search by Name or SKU
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            productsQuery = productsQuery
                .Where(p => p.Name.ToLower().Contains(search)
                         || p.SKU.ToLower().Contains(search));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            if (query.SortBy.ToLower() == "price")
            {
                productsQuery = query.SortOrder == "desc"
                    ? productsQuery.OrderByDescending(p => p.Price)
                    : productsQuery.OrderBy(p => p.Price);
            }
        }

        // Pagination
        productsQuery = productsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);

        return await productsQuery
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                Quantity = p.Inventory != null ? p.Inventory.Quantity : 0
            })
            .ToListAsync();
    }

}