using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Auth.Application.DTOs;
using SwiftApp.ERP.Modules.Auth.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Auth.Controllers;

/// <summary>
/// User management CRUD endpoints.
/// Maps to Java: UserController.
/// </summary>
[ApiController]
[Route("api/v1/auth/users")]
[Produces("application/json")]
[Authorize(Policy = "ADMIN:USERS_MANAGE")]
public class UserController(
    UserService userService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<UserResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await userService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [ProducesResponseType<UserResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] UserRequest request, CancellationToken ct)
    {
        var user = await userService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<UserResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserRequest request, CancellationToken ct)
    {
        var user = await userService.UpdateAsync(id, request, ct);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await userService.DeleteAsync(id, ct);
        return NoContent();
    }
}
