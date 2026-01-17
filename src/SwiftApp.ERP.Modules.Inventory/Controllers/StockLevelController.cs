using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Inventory.Application.DTOs;
using SwiftApp.ERP.Modules.Inventory.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Controllers;

[ApiController]
[Route("api/v1/inventory/stock-levels")]
[Produces("application/json")]
public class StockLevelController(StockService stockService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<PagedResult<StockLevelResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await stockService.GetStockLevelsPagedAsync(page, size, ct);
        return Ok(result);
    }

    [HttpGet("by-item/{itemId:guid}")]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<IReadOnlyList<StockLevelResponse>>(200)]
    public async Task<IActionResult> GetByItem(
        Guid itemId,
        [FromQuery] string itemType = "Product",
        CancellationToken ct = default)
    {
        var result = await stockService.GetStockLevelsByItemAsync(itemId, itemType, ct);
        return Ok(result);
    }
}
