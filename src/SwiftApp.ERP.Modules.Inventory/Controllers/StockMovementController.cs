using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Inventory.Application.DTOs;
using SwiftApp.ERP.Modules.Inventory.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Controllers;

[ApiController]
[Route("api/v1/inventory/stock-movements")]
[Produces("application/json")]
public class StockMovementController(StockService stockService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<PagedResult<StockMovementResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await stockService.GetMovementsPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<StockMovementResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var movement = await stockService.GetMovementByIdAsync(id, ct);
        return movement is null ? NotFound() : Ok(movement);
    }

    [HttpPost]
    [Authorize(Policy = "INVENTORY:CREATE")]
    [ProducesResponseType<StockMovementResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> RecordMovement([FromBody] StockMovementRequest request, CancellationToken ct = default)
    {
        var movement = await stockService.RecordMovementAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }
}
