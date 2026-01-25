using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Controllers;

[ApiController]
[Route("api/v1/quality-control/quality-checks")]
[Produces("application/json")]
public class QualityCheckController(QualityCheckService checkService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<PagedResult<QualityCheckResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await checkService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<QualityCheckResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var check = await checkService.GetByIdAsync(id, ct);
        return check is null ? NotFound() : Ok(check);
    }

    [HttpGet("by-number/{checkNumber}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<QualityCheckResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCheckNumber(string checkNumber, CancellationToken ct = default)
    {
        var check = await checkService.GetByCheckNumberAsync(checkNumber, ct);
        return check is null ? NotFound() : Ok(check);
    }

    [HttpGet("by-inspection-plan/{inspectionPlanId:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<IReadOnlyList<QualityCheckResponse>>(200)]
    public async Task<IActionResult> GetByInspectionPlan(Guid inspectionPlanId, CancellationToken ct = default)
    {
        var result = await checkService.GetByInspectionPlanAsync(inspectionPlanId, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "QUALITY_CONTROL:CREATE")]
    [ProducesResponseType<QualityCheckResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] QualityCheckRequest request, CancellationToken ct = default)
    {
        var check = await checkService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = check.Id }, check);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:EDIT")]
    [ProducesResponseType<QualityCheckResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] QualityCheckRequest request, CancellationToken ct = default)
    {
        var check = await checkService.UpdateAsync(id, request, ct);
        return Ok(check);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await checkService.DeleteAsync(id, ct);
        return NoContent();
    }
}
