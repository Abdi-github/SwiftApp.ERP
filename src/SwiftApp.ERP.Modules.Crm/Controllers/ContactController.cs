using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Crm.Application.DTOs;
using SwiftApp.ERP.Modules.Crm.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Crm.Controllers;

[ApiController]
[Route("api/v1/crm/contacts")]
[Produces("application/json")]
public class ContactController(ContactService contactService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CRM:VIEW")]
    [ProducesResponseType<PagedResult<ContactResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await contactService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CRM:VIEW")]
    [ProducesResponseType<ContactResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var contact = await contactService.GetByIdAsync(id, ct);
        return contact is null ? NotFound() : Ok(contact);
    }

    [HttpGet("by-customer/{customerId:guid}")]
    [Authorize(Policy = "CRM:VIEW")]
    [ProducesResponseType<IReadOnlyList<ContactResponse>>(200)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken ct = default)
    {
        var result = await contactService.GetByCustomerIdAsync(customerId, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CRM:CREATE")]
    [ProducesResponseType<ContactResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] ContactRequest request, CancellationToken ct = default)
    {
        var contact = await contactService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CRM:EDIT")]
    [ProducesResponseType<ContactResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ContactRequest request, CancellationToken ct = default)
    {
        var contact = await contactService.UpdateAsync(id, request, ct);
        return Ok(contact);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CRM:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await contactService.DeleteAsync(id, ct);
        return NoContent();
    }
}
