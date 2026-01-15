using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Hr.Application.DTOs;
using SwiftApp.ERP.Modules.Hr.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Hr.Controllers;

[ApiController]
[Route("api/v1/hr/employees")]
[Produces("application/json")]
public class EmployeeController(EmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<PagedResult<EmployeeResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await employeeService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("active")]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(200)]
    public async Task<IActionResult> GetActive(CancellationToken ct = default)
    {
        var result = await employeeService.GetActiveAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<EmployeeResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var employee = await employeeService.GetByIdAsync(id, ct);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpGet("by-number/{employeeNumber}")]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<EmployeeResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByEmployeeNumber(string employeeNumber, CancellationToken ct = default)
    {
        var employee = await employeeService.GetByEmployeeNumberAsync(employeeNumber, ct);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpGet("by-department/{departmentId:guid}")]
    [Authorize(Policy = "HR:VIEW")]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(200)]
    public async Task<IActionResult> GetByDepartment(Guid departmentId, CancellationToken ct = default)
    {
        var result = await employeeService.GetByDepartmentAsync(departmentId, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "HR:CREATE")]
    [ProducesResponseType<EmployeeResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] EmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await employeeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "HR:EDIT")]
    [ProducesResponseType<EmployeeResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await employeeService.UpdateAsync(id, request, ct);
        return Ok(employee);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "HR:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await employeeService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/terminate")]
    [Authorize(Policy = "HR:EDIT")]
    [ProducesResponseType<EmployeeResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] TerminateRequest request, CancellationToken ct = default)
    {
        var employee = await employeeService.TerminateAsync(id, request.TerminationDate, ct);
        return Ok(employee);
    }
}

public record TerminateRequest(DateOnly TerminationDate);
