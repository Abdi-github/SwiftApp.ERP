using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Purchasing.Application.DTOs;
using SwiftApp.ERP.Modules.Purchasing.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Purchasing.Controllers;

[ApiController]
[Route("api/v1/purchasing/suppliers")]
[Produces("application/json")]
public class SupplierController(SupplierService supplierService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "PURCHASING:VIEW")]
    [ProducesResponseType<PagedResult<SupplierResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await supplierService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "PURCHASING:VIEW")]
    [ProducesResponseType<SupplierResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var supplier = await supplierService.GetByIdAsync(id, ct);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpGet("by-number/{supplierNumber}")]
    [Authorize(Policy = "PURCHASING:VIEW")]
    [ProducesResponseType<SupplierResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySupplierNumber(string supplierNumber, CancellationToken ct = default)
    {
        var supplier = await supplierService.GetBySupplierNumberAsync(supplierNumber, ct);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [Authorize(Policy = "PURCHASING:CREATE")]
    [ProducesResponseType<SupplierResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] SupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await supplierService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "PURCHASING:EDIT")]
    [ProducesResponseType<SupplierResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await supplierService.UpdateAsync(id, request, ct);
        return Ok(supplier);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "PURCHASING:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await supplierService.DeleteAsync(id, ct);
        return NoContent();
    }
}
