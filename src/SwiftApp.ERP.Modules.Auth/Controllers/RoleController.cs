using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Auth.Application.DTOs;
using SwiftApp.ERP.Modules.Auth.Application.Services;

namespace SwiftApp.ERP.Modules.Auth.Controllers;

/// <summary>
/// Role management CRUD endpoints.
/// Maps to Java: RoleController.
/// </summary>
[ApiController]
[Route("api/v1/auth/roles")]
[Produces("application/json")]
[Authorize(Policy = "ADMIN:ROLES_MANAGE")]
public class RoleController(
    RoleService roleService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<RoleResponse>>(200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await roleService.GetAllAsync(ct);
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RoleResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var role = await roleService.GetByIdAsync(id, ct);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpGet("permissions")]
    [ProducesResponseType<Dictionary<string, List<PermissionResponse>>>(200)]
    public async Task<IActionResult> GetPermissionsGrouped(CancellationToken ct)
    {
        var grouped = await roleService.GetPermissionsGroupedByModuleAsync(ct);
        return Ok(grouped);
    }

    [HttpPost]
    [ProducesResponseType<RoleResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] RoleRequest request, CancellationToken ct)
    {
        var role = await roleService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<RoleResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoleRequest request, CancellationToken ct)
    {
        var role = await roleService.UpdateAsync(id, request, ct);
        return Ok(role);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await roleService.DeleteAsync(id, ct);
        return NoContent();
    }
}
