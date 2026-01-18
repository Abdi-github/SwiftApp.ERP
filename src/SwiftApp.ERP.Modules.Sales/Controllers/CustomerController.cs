using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Sales.Application.DTOs;
using SwiftApp.ERP.Modules.Sales.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Sales.Controllers;

[ApiController]
[Route("api/v1/sales/customers")]
[Produces("application/json")]
public class CustomerController(CustomerService customerService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "SALES:VIEW")]
    [ProducesResponseType<PagedResult<CustomerResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await customerService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SALES:VIEW")]
    [ProducesResponseType<CustomerResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var customer = await customerService.GetByIdAsync(id, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("by-number/{customerNumber}")]
    [Authorize(Policy = "SALES:VIEW")]
    [ProducesResponseType<CustomerResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCustomerNumber(string customerNumber, CancellationToken ct = default)
    {
        var customer = await customerService.GetByCustomerNumberAsync(customerNumber, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    [Authorize(Policy = "SALES:CREATE")]
    [ProducesResponseType<CustomerResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] CustomerRequest request, CancellationToken ct = default)
    {
        var customer = await customerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SALES:EDIT")]
    [ProducesResponseType<CustomerResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerRequest request, CancellationToken ct = default)
    {
        var customer = await customerService.UpdateAsync(id, request, ct);
        return Ok(customer);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SALES:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await customerService.DeleteAsync(id, ct);
        return NoContent();
    }
}
