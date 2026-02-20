using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PIMS.Application.DTOs.Product;
using PIMS.Application.Exceptions;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

namespace PIMS.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;

    private const string ProductsCacheKey = "products_all";

    public ProductService(ApplicationDbContext context,
                          IMemoryCache cache,
                          ILogger<ProductService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    #region Create Product

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        _logger.LogInformation("Creating product with SKU: {SKU}", dto.SKU);

        if (await _context.Products.AnyAsync(p => p.SKU == dto.SKU))
        {
            _logger.LogWarning("SKU already exists: {SKU}", dto.SKU);
            throw new BadRequestException("SKU already exists.");
        }

        var categories = await _context.Categories
            .Where(c => dto.CategoryIds.Contains(c.Id))
            .ToListAsync();

        if (categories.Count != dto.CategoryIds.Count)
        {
            _logger.LogWarning("Invalid category IDs provided.");
            throw new BadRequestException("One or more categories not found.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = Math.Round(dto.Price, 2),
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

        _cache.Remove(ProductsCacheKey);

        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Price = product.Price,
            Quantity = 0
        };
    }

    #endregion

    #region Get All Products (Cached)

    public async Task<List<ProductResponseDto>> GetAllProductsAsync()
    {
        if (_cache.TryGetValue(ProductsCacheKey, out List<ProductResponseDto>? cachedProducts))
        {
            _logger.LogInformation("Products fetched from cache.");
            return cachedProducts!;
        }

        _logger.LogInformation("Fetching products from database.");

        var products = await _context.Products
            .AsNoTracking()
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                Quantity = p.Inventory != null ? p.Inventory.Quantity : 0
            })
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(ProductsCacheKey, products, cacheOptions);

        _logger.LogInformation("Products cached successfully.");

        return products;
    }

    #endregion

    #region Filtered Products (No Cache)

    public async Task<List<ProductResponseDto>> GetProductsAsync(ProductQueryDto query)
    {
        _logger.LogInformation("Fetching products with filters.");

        var productsQuery = _context.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .AsNoTracking()
            .AsQueryable();

        // Filter by Category
        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery
                .Where(p => p.ProductCategories
                    .Any(pc => pc.CategoryId == query.CategoryId));
        }

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            productsQuery = productsQuery
                .Where(p => p.Name.ToLower().Contains(search)
                         || p.SKU.ToLower().Contains(search));
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy) &&
            query.SortBy.ToLower() == "price")
        {
            productsQuery = query.SortOrder == "desc"
                ? productsQuery.OrderByDescending(p => p.Price)
                : productsQuery.OrderBy(p => p.Price);
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

    #endregion
}