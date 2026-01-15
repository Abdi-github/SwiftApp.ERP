using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Hr.Application.DTOs;
using SwiftApp.ERP.Modules.Hr.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Hr.Controllers;

[ApiController]
[Route("api/v1/hr/departments")]
[Produces("application/json")]
public class DepartmentController(DepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<PagedResult<DepartmentResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await departmentService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("active")]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<IReadOnlyList<DepartmentResponse>>(200)]
    public async Task<IActionResult> GetAllActive(CancellationToken ct = default)
    {
        var result = await departmentService.GetAllActiveAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<DepartmentResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var department = await departmentService.GetByIdAsync(id, ct);
        return department is null ? NotFound() : Ok(department);
    }

    [HttpGet("by-code/{code}")]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<DepartmentResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct = default)
    {
        var department = await departmentService.GetByCodeAsync(code, ct);
        return department is null ? NotFound() : Ok(department);
    }

    [HttpPost]
    [Authorize(Policy = "HR:CREATE")]
    [ProducesResponseType<DepartmentResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] DepartmentRequest request, CancellationToken ct = default)
    {
        var department = await departmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "HR:EDIT")]
    [ProducesResponseType<DepartmentResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] DepartmentRequest request, CancellationToken ct = default)
    {
        var department = await departmentService.UpdateAsync(id, request, ct);
        return Ok(department);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "HR:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await departmentService.DeleteAsync(id, ct);
        return NoContent();
    }
}
