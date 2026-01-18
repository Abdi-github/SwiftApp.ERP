using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Crm.Application.DTOs;
using SwiftApp.ERP.Modules.Crm.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Crm.Controllers;

[ApiController]
[Route("api/v1/crm/interactions")]
[Produces("application/json")]
public class InteractionController(InteractionService interactionService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CRM:VIEW")]
    [ProducesResponseType<PagedResult<InteractionResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await interactionService.GetPagedAsync(page, size, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CRM:VIEW")]
    [ProducesResponseType<InteractionResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var interaction = await interactionService.GetByIdAsync(id, ct);
        return interaction is null ? NotFound() : Ok(interaction);
    }

    [HttpGet("by-contact/{contactId:guid}")]
    [Authorize(Policy = "CRM:VIEW")]
    [ProducesResponseType<IReadOnlyList<InteractionResponse>>(200)]
    public async Task<IActionResult> GetByContact(Guid contactId, CancellationToken ct = default)
    {
        var result = await interactionService.GetByContactAsync(contactId, ct);
        return Ok(result);
    }

    [HttpGet("upcoming")]
    [Authorize(Policy = "CRM:VIEW")]
    [ProducesResponseType<IReadOnlyList<InteractionResponse>>(200)]
    public async Task<IActionResult> GetUpcoming([FromQuery] DateTimeOffset? from = null, CancellationToken ct = default)
    {
        var result = await interactionService.GetUpcomingAsync(from, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CRM:CREATE")]
    [ProducesResponseType<InteractionResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] InteractionRequest request, CancellationToken ct = default)
    {
        var interaction = await interactionService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = interaction.Id }, interaction);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CRM:EDIT")]
    [ProducesResponseType<InteractionResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] InteractionRequest request, CancellationToken ct = default)
    {
        var interaction = await interactionService.UpdateAsync(id, request, ct);
        return Ok(interaction);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CRM:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await interactionService.DeleteAsync(id, ct);
        return NoContent();
    }
}
