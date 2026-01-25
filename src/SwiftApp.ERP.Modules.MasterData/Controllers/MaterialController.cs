using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Controllers;

[ApiController]
[Route("api/v1/masterdata/materials")]
[Produces("application/json")]
public class MaterialController(MaterialService materialService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<PagedResult<MaterialResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await materialService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<MaterialResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var material = await materialService.GetByIdAsync(id, ct);
        return material is null ? NotFound() : Ok(material);
    }

    [HttpGet("by-sku/{sku}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<MaterialResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken ct = default)
    {
        var material = await materialService.GetBySkuAsync(sku, ct);
        return material is null ? NotFound() : Ok(material);
    }

    [HttpPost]
    [Authorize(Policy = "MASTERDATA:CREATE")]
    [ProducesResponseType<MaterialResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] MaterialRequest request, CancellationToken ct = default)
    {
        var material = await materialService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:EDIT")]
    [ProducesResponseType<MaterialResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] MaterialRequest request, CancellationToken ct = default)
    {
        var material = await materialService.UpdateAsync(id, request, ct);
        return Ok(material);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await materialService.DeleteAsync(id, ct);
        return NoContent();
    }
}
