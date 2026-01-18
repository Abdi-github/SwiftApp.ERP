using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Sales.Application.DTOs;
using SwiftApp.ERP.Modules.Sales.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Sales.Controllers;

[ApiController]
[Route("api/v1/sales/orders")]
[Produces("application/json")]
public class SalesOrderController(SalesOrderService orderService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "SALES:VIEW")]
    [ProducesResponseType<PagedResult<SalesOrderResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await orderService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SALES:VIEW")]
    [ProducesResponseType<SalesOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-number/{orderNumber}")]
    [Authorize(Policy = "SALES:VIEW")]
    [ProducesResponseType<SalesOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken ct = default)
    {
        var order = await orderService.GetByOrderNumberAsync(orderNumber, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-customer/{customerId:guid}")]
    [Authorize(Policy = "SALES:VIEW")]
    [ProducesResponseType<PagedResult<SalesOrderResponse>>(200)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
    {
        var result = await orderService.GetByCustomerAsync(customerId, page, size, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "SALES:CREATE")]
    [ProducesResponseType<SalesOrderResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] SalesOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SALES:EDIT")]
    [ProducesResponseType<SalesOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SalesOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.UpdateAsync(id, request, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = "SALES:APPROVE")]
    [ProducesResponseType<SalesOrderResponse>(200)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.ConfirmAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/advance-status")]
    [Authorize(Policy = "SALES:EDIT")]
    [ProducesResponseType<SalesOrderResponse>(200)]
    public async Task<IActionResult> AdvanceStatus(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.AdvanceStatusAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "SALES:EDIT")]
    [ProducesResponseType<SalesOrderResponse>(200)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.CancelAsync(id, request.Reason, ct);
        return Ok(order);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SALES:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await orderService.DeleteAsync(id, ct);
        return NoContent();
    }
}

public record CancelOrderRequest(string Reason);
