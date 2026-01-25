using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Controllers;

[ApiController]
[Route("api/v1/quality-control/ncrs")]
[Produces("application/json")]
public class NonConformanceReportController(NonConformanceReportService ncrService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<PagedResult<NcrResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await ncrService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<NcrResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var ncr = await ncrService.GetByIdAsync(id, ct);
        return ncr is null ? NotFound() : Ok(ncr);
    }

    [HttpGet("by-number/{ncrNumber}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<NcrResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByNcrNumber(string ncrNumber, CancellationToken ct = default)
    {
        var ncr = await ncrService.GetByNcrNumberAsync(ncrNumber, ct);
        return ncr is null ? NotFound() : Ok(ncr);
    }

    [HttpGet("by-quality-check/{qualityCheckId:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<IReadOnlyList<NcrResponse>>(200)]
    public async Task<IActionResult> GetByQualityCheck(Guid qualityCheckId, CancellationToken ct = default)
    {
        var result = await ncrService.GetByQualityCheckAsync(qualityCheckId, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "QUALITY_CONTROL:CREATE")]
    [ProducesResponseType<NcrResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] NcrRequest request, CancellationToken ct = default)
    {
        var ncr = await ncrService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = ncr.Id }, ncr);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:EDIT")]
    [ProducesResponseType<NcrResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] NcrRequest request, CancellationToken ct = default)
    {
        var ncr = await ncrService.UpdateAsync(id, request, ct);
        return Ok(ncr);
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = "QUALITY_CONTROL:CREATE")]
    [ProducesResponseType<NcrResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct = default)
    {
        var ncr = await ncrService.StartAsync(id, ct);
        return Ok(ncr);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "QUALITY_CONTROL:CREATE")]
    [ProducesResponseType<NcrResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct = default)
    {
        var ncr = await ncrService.CloseAsync(id, ct);
        return Ok(ncr);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await ncrService.DeleteAsync(id, ct);
        return NoContent();
    }
}
