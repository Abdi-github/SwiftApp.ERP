using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;

namespace SwiftApp.ERP.Modules.MasterData.Controllers;

[ApiController]
[Route("api/v1/masterdata/units-of-measure")]
[Produces("application/json")]
public class UnitOfMeasureController(UnitOfMeasureService uomService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<List<UnitOfMeasureResponse>>(200)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await uomService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<UnitOfMeasureResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var uom = await uomService.GetByIdAsync(id, ct);
        return uom is null ? NotFound() : Ok(uom);
    }

    [HttpGet("by-code/{code}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<UnitOfMeasureResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct = default)
    {
        var uom = await uomService.GetByCodeAsync(code, ct);
        return uom is null ? NotFound() : Ok(uom);
    }

    [HttpPost]
    [Authorize(Policy = "MASTERDATA:CREATE")]
    [ProducesResponseType<UnitOfMeasureResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] UnitOfMeasureRequest request, CancellationToken ct = default)
    {
        var uom = await uomService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = uom.Id }, uom);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:EDIT")]
    [ProducesResponseType<UnitOfMeasureResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UnitOfMeasureRequest request, CancellationToken ct = default)
    {
        var uom = await uomService.UpdateAsync(id, request, ct);
        return Ok(uom);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await uomService.DeleteAsync(id, ct);
        return NoContent();
    }
}
