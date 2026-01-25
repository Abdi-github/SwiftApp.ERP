using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Controllers;

[ApiController]
[Route("api/v1/quality-control/inspection-plans")]
[Produces("application/json")]
public class InspectionPlanController(InspectionPlanService planService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<PagedResult<InspectionPlanResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await planService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("active")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<IReadOnlyList<InspectionPlanResponse>>(200)]
    public async Task<IActionResult> GetActive(CancellationToken ct = default)
    {
        var result = await planService.GetActiveAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<InspectionPlanResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var plan = await planService.GetByIdAsync(id, ct);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpGet("by-number/{planNumber}")]
    [Authorize(Policy = "QUALITY_CONTROL:VIEW")]
    [ProducesResponseType<InspectionPlanResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByPlanNumber(string planNumber, CancellationToken ct = default)
    {
        var plan = await planService.GetByPlanNumberAsync(planNumber, ct);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    [Authorize(Policy = "QUALITY_CONTROL:CREATE")]
    [ProducesResponseType<InspectionPlanResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] InspectionPlanRequest request, CancellationToken ct = default)
    {
        var plan = await planService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:EDIT")]
    [ProducesResponseType<InspectionPlanResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] InspectionPlanRequest request, CancellationToken ct = default)
    {
        var plan = await planService.UpdateAsync(id, request, ct);
        return Ok(plan);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "QUALITY_CONTROL:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await planService.DeleteAsync(id, ct);
        return NoContent();
    }
}
