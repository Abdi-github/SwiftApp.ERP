using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Inventory.Application.DTOs;
using SwiftApp.ERP.Modules.Inventory.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Controllers;

[ApiController]
[Route("api/v1/inventory/warehouses")]
[Produces("application/json")]
public class WarehouseController(WarehouseService warehouseService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<PagedResult<WarehouseResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await warehouseService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("active")]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<IReadOnlyList<WarehouseResponse>>(200)]
    public async Task<IActionResult> GetAllActive(CancellationToken ct = default)
    {
        var result = await warehouseService.GetAllActiveAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<WarehouseResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var warehouse = await warehouseService.GetByIdAsync(id, ct);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpGet("by-code/{code}")]
    [Authorize(Policy = "INVENTORY:VIEW")]
    [ProducesResponseType<WarehouseResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct = default)
    {
        var warehouse = await warehouseService.GetByCodeAsync(code, ct);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpPost]
    [Authorize(Policy = "INVENTORY:CREATE")]
    [ProducesResponseType<WarehouseResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] WarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await warehouseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "INVENTORY:EDIT")]
    [ProducesResponseType<WarehouseResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] WarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await warehouseService.UpdateAsync(id, request, ct);
        return Ok(warehouse);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "INVENTORY:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await warehouseService.DeleteAsync(id, ct);
        return NoContent();
    }
}
