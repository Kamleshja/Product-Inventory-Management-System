using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PIMS.Application.DTOs.Inventory;
using PIMS.Application.Interfaces;

namespace PIMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("adjust")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdjustStock(InventoryAdjustmentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _inventoryService.AdjustInventoryAsync(dto, userId);

        return Ok(result);
    }

    [HttpGet("history")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid? productId)
    {
        var transactions = await _inventoryService
            .GetTransactionHistoryAsync(productId);

        return Ok(transactions);
    }
    [HttpGet("low-stock")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var result = await _inventoryService.GetLowStockProductsAsync();
        return Ok(result);
    }
}
