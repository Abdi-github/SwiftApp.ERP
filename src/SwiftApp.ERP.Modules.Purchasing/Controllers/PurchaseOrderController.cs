using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Purchasing.Application.DTOs;
using SwiftApp.ERP.Modules.Purchasing.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Purchasing.Controllers;

[ApiController]
[Route("api/v1/purchasing/orders")]
[Produces("application/json")]
public class PurchaseOrderController(PurchaseOrderService orderService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "PURCHASING:VIEW")]
    [ProducesResponseType<PagedResult<PurchaseOrderResponse>>(200)]
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
    [Authorize(Policy = "PURCHASING:VIEW")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-number/{orderNumber}")]
    [Authorize(Policy = "PURCHASING:VIEW")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken ct = default)
    {
        var order = await orderService.GetByOrderNumberAsync(orderNumber, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-supplier/{supplierId:guid}")]
    [Authorize(Policy = "PURCHASING:VIEW")]
    [ProducesResponseType<PagedResult<PurchaseOrderResponse>>(200)]
    public async Task<IActionResult> GetBySupplier(Guid supplierId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
    {
        var result = await orderService.GetBySupplierAsync(supplierId, page, size, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "PURCHASING:CREATE")]
    [ProducesResponseType<PurchaseOrderResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "PURCHASING:EDIT")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PurchaseOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.UpdateAsync(id, request, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "PURCHASING:EDIT")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.SubmitAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = "PURCHASING:APPROVE")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.ConfirmAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = "PURCHASING:EDIT")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    public async Task<IActionResult> Receive(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.ReceiveAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "PURCHASING:EDIT")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.CompleteAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "PURCHASING:EDIT")]
    [ProducesResponseType<PurchaseOrderResponse>(200)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelPurchaseOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.CancelAsync(id, request.Reason, ct);
        return Ok(order);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "PURCHASING:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await orderService.DeleteAsync(id, ct);
        return NoContent();
    }
}

public record CancelPurchaseOrderRequest(string Reason);
