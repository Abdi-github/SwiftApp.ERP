using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Accounting.Application.DTOs;
using SwiftApp.ERP.Modules.Accounting.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Accounting.Controllers;

[ApiController]
[Route("api/v1/accounting/journal-entries")]
[Produces("application/json")]
public class JournalEntryController(JournalEntryService entryService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<PagedResult<JournalEntryResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await entryService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<JournalEntryResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var entry = await entryService.GetByIdAsync(id, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpGet("by-number/{entryNumber}")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<JournalEntryResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByEntryNumber(string entryNumber, CancellationToken ct = default)
    {
        var entry = await entryService.GetByEntryNumberAsync(entryNumber, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpGet("unposted")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<List<JournalEntryResponse>>(200)]
    public async Task<IActionResult> GetUnposted(CancellationToken ct = default)
    {
        var entries = await entryService.GetUnpostedAsync(ct);
        return Ok(entries);
    }

    [HttpPost]
    [Authorize(Policy = "ACCOUNTING:CREATE")]
    [ProducesResponseType<JournalEntryResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] JournalEntryRequest request, CancellationToken ct = default)
    {
        var entry = await entryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ACCOUNTING:EDIT")]
    [ProducesResponseType<JournalEntryResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] JournalEntryRequest request, CancellationToken ct = default)
    {
        var entry = await entryService.UpdateAsync(id, request, ct);
        return Ok(entry);
    }

    [HttpPost("{id:guid}/post")]
    [Authorize(Policy = "ACCOUNTING:APPROVE")]
    [ProducesResponseType<JournalEntryResponse>(200)]
    public async Task<IActionResult> Post(Guid id, CancellationToken ct = default)
    {
        var entry = await entryService.PostAsync(id, ct);
        return Ok(entry);
    }

    [HttpPost("{id:guid}/reverse")]
    [Authorize(Policy = "ACCOUNTING:APPROVE")]
    [ProducesResponseType<JournalEntryResponse>(200)]
    public async Task<IActionResult> Reverse(Guid id, CancellationToken ct = default)
    {
        var entry = await entryService.ReverseAsync(id, ct);
        return Ok(entry);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ACCOUNTING:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await entryService.DeleteAsync(id, ct);
        return NoContent();
    }
}
