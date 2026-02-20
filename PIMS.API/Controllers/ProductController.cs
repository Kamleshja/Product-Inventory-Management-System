using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIMS.Application.DTOs.Product;
using PIMS.Application.Interfaces;
using PIMS.Infrastructure.Services;

namespace PIMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IPriceService _priceService;

    public ProductController(IProductService productService, IPriceService priceService)
    {
        _productService = productService;
        _priceService = priceService;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryDto query)
    {
        var products = await _productService.GetProductsAsync(query);
        return Ok(products);
    }
    [HttpPut("price")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdatePrice(UpdateProductPriceDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        await _priceService.UpdatePriceAsync(request, userId);

        return Ok(new { Success = true, Message = "Price updated successfully." });
    }
    [HttpPut("price/bulk")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> BulkUpdatePrice(BulkPriceUpdateDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        await _priceService.BulkUpdatePriceAsync(request, userId);

        return Ok(new { Success = true, Message = "Bulk price update successful." });
    }
}