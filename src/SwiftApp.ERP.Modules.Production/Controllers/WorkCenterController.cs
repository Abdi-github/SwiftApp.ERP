using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Production.Application.DTOs;
using SwiftApp.ERP.Modules.Production.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Controllers;

[ApiController]
[Route("api/v1/production/work-centers")]
[Produces("application/json")]
public class WorkCenterController(WorkCenterService workCenterService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<PagedResult<WorkCenterResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await workCenterService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("active")]
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<IReadOnlyList<WorkCenterResponse>>(200)]
    public async Task<IActionResult> GetAllActive(CancellationToken ct = default)
    {
        var result = await workCenterService.GetAllActiveAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<WorkCenterResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var wc = await workCenterService.GetByIdAsync(id, ct);
        return wc is null ? NotFound() : Ok(wc);
    }

    [HttpGet("by-code/{code}")]
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<WorkCenterResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct = default)
    {
        var wc = await workCenterService.GetByCodeAsync(code, ct);
        return wc is null ? NotFound() : Ok(wc);
    }

    [HttpPost]
    [Authorize(Policy = "PRODUCTION:CREATE")]
    [ProducesResponseType<WorkCenterResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] WorkCenterRequest request, CancellationToken ct = default)
    {
        var wc = await workCenterService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = wc.Id }, wc);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<WorkCenterResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] WorkCenterRequest request, CancellationToken ct = default)
    {
        var wc = await workCenterService.UpdateAsync(id, request, ct);
        return Ok(wc);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "PRODUCTION:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await workCenterService.DeleteAsync(id, ct);
        return NoContent();
    }
}
