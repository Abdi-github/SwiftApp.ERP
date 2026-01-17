using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Production.Application.DTOs;
using SwiftApp.ERP.Modules.Production.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Controllers;

[ApiController]
[Route("api/v1/production/orders")]
[Produces("application/json")]
public class ProductionOrderController(ProductionOrderService orderService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<PagedResult<ProductionOrderResponse>>(200)]
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
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-number/{orderNumber}")]
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken ct = default)
    {
        var order = await orderService.GetByOrderNumberAsync(orderNumber, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-work-center/{workCenterId:guid}")]
    [Authorize(Policy = "PRODUCTION:VIEW")]
    [ProducesResponseType<PagedResult<ProductionOrderResponse>>(200)]
    public async Task<IActionResult> GetByWorkCenter(Guid workCenterId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
    {
        var result = await orderService.GetByWorkCenterAsync(workCenterId, page, size, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "PRODUCTION:CREATE")]
    [ProducesResponseType<ProductionOrderResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] ProductionOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductionOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.UpdateAsync(id, request, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/release")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    public async Task<IActionResult> Release(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.ReleaseAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.StartAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteProductionOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.CompleteAsync(id, request.CompletedQuantity, request.ScrapQuantity, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelProductionOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderService.CancelAsync(id, request.Reason, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/hold")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    public async Task<IActionResult> Hold(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.HoldAsync(id, ct);
        return Ok(order);
    }

    [HttpPost("{id:guid}/resume")]
    [Authorize(Policy = "PRODUCTION:EDIT")]
    [ProducesResponseType<ProductionOrderResponse>(200)]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct = default)
    {
        var order = await orderService.ResumeAsync(id, ct);
        return Ok(order);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "PRODUCTION:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await orderService.DeleteAsync(id, ct);
        return NoContent();
    }
}
