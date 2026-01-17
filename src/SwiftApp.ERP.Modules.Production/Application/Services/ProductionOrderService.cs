using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Production.Application.DTOs;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Enums;
using SwiftApp.ERP.Modules.Production.Domain.Events;
using SwiftApp.ERP.Modules.Production.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Production.Application.Services;

public class ProductionOrderService(
    IProductionOrderRepository orderRepository,
    IProductionOrderLineRepository lineRepository,
    IWorkCenterRepository workCenterRepository,
    IPublisher publisher,
    ILogger<ProductionOrderService> logger)
{
    public async Task<ProductionOrderResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<ProductionOrderResponse?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByOrderNumberAsync(orderNumber, ct);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<PagedResult<ProductionOrderResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await orderRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<ProductionOrderResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<PagedResult<ProductionOrderResponse>> GetByWorkCenterAsync(Guid workCenterId, int page, int size, CancellationToken ct = default)
    {
        var result = await orderRepository.GetByWorkCenterAsync(workCenterId, page, size, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<ProductionOrderResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<ProductionOrderResponse> CreateAsync(ProductionOrderRequest request, CancellationToken ct = default)
    {
        if (request.WorkCenterId.HasValue)
        {
            _ = await workCenterRepository.GetByIdAsync(request.WorkCenterId.Value, ct)
                ?? throw new EntityNotFoundException(nameof(WorkCenter), request.WorkCenterId.Value);
        }

        var year = DateTime.UtcNow.Year;
        var seq = await orderRepository.GetMaxSequenceForYearAsync(year, ct) + 1;
        var orderNumber = $"MO-{year}-{seq:D5}";

        var order = new ProductionOrder
        {
            OrderNumber = orderNumber,
            ProductId = request.ProductId,
            WorkCenterId = request.WorkCenterId,
            PlannedQuantity = request.PlannedQuantity,
            PlannedStartDate = request.PlannedStartDate,
            PlannedEndDate = request.PlannedEndDate,
            Notes = request.Notes,
        };

        if (request.Lines is { Count: > 0 })
        {
            var position = 0;
            foreach (var lineReq in request.Lines)
            {
                order.Lines.Add(new ProductionOrderLine
                {
                    MaterialId = lineReq.MaterialId,
                    Description = lineReq.Description,
                    RequiredQuantity = lineReq.RequiredQuantity,
                    Position = lineReq.Position ?? position,
                });
                position++;
            }
        }

        await orderRepository.AddAsync(order, ct);
        // System.Diagnostics.Debug.WriteLine($"Production order persisted: id={order.Id}, lines={order.Lines.Count}");

        logger.LogInformation("Production order {OrderNumber} created for product {ProductId}", order.OrderNumber, order.ProductId);
        await publisher.Publish(new ProductionOrderCreatedEvent(order.Id, order.OrderNumber, order.ProductId), ct);

        return MapToResponse(order);
    }

    public async Task<ProductionOrderResponse> UpdateAsync(Guid id, ProductionOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status != ProductionOrderStatus.Draft)
            throw new BusinessRuleException("ORDER_NOT_DRAFT", "Only DRAFT orders can be updated.");

        if (request.WorkCenterId.HasValue)
        {
            _ = await workCenterRepository.GetByIdAsync(request.WorkCenterId.Value, ct)
                ?? throw new EntityNotFoundException(nameof(WorkCenter), request.WorkCenterId.Value);
        }

        order.ProductId = request.ProductId;
        order.WorkCenterId = request.WorkCenterId;
        order.PlannedQuantity = request.PlannedQuantity;
        order.PlannedStartDate = request.PlannedStartDate;
        order.PlannedEndDate = request.PlannedEndDate;
        order.Notes = request.Notes;

        // Replace all lines
        await lineRepository.DeleteByOrderIdAsync(id, ct);
        order.Lines.Clear();

        if (request.Lines is { Count: > 0 })
        {
            var position = 0;
            foreach (var lineReq in request.Lines)
            {
                order.Lines.Add(new ProductionOrderLine
                {
                    ProductionOrderId = id,
                    MaterialId = lineReq.MaterialId,
                    Description = lineReq.Description,
                    RequiredQuantity = lineReq.RequiredQuantity,
                    Position = lineReq.Position ?? position,
                });
                position++;
            }
        }

        await orderRepository.UpdateAsync(order, ct);
        logger.LogInformation("Production order {OrderNumber} updated", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task<ProductionOrderResponse> ReleaseAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status != ProductionOrderStatus.Draft)
            throw new BusinessRuleException("ORDER_NOT_DRAFT", "Only DRAFT orders can be released.");

        if (order.Lines.Count == 0)
            throw new BusinessRuleException("ORDER_NO_LINES", "Order must have at least one line to be released.");

        order.Status = ProductionOrderStatus.Released;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Production order {OrderNumber} released", order.OrderNumber);
        await publisher.Publish(new ProductionOrderReleasedEvent(order.Id, order.OrderNumber), ct);

        return MapToResponse(order);
    }

    public async Task<ProductionOrderResponse> StartAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status != ProductionOrderStatus.Released)
            throw new BusinessRuleException("ORDER_NOT_RELEASED", "Only RELEASED orders can be started.");

        order.Status = ProductionOrderStatus.InProgress;
        order.ActualStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Production order {OrderNumber} started", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task<ProductionOrderResponse> CompleteAsync(Guid id, decimal completedQuantity, decimal? scrapQuantity, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status != ProductionOrderStatus.InProgress)
            throw new BusinessRuleException("ORDER_NOT_IN_PROGRESS", "Only IN_PROGRESS orders can be completed.");

        order.Status = ProductionOrderStatus.Completed;
        order.CompletedQuantity = completedQuantity;
        order.ScrapQuantity = scrapQuantity ?? 0m;
        // logger.LogDebug("Completing production order {OrderNumber}: completed={CompletedQuantity}, scrap={ScrapQuantity}", order.OrderNumber, completedQuantity, scrapQuantity ?? 0m);
        order.ActualEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Production order {OrderNumber} completed. Qty: {CompletedQuantity}, Scrap: {ScrapQuantity}",
            order.OrderNumber, order.CompletedQuantity, order.ScrapQuantity);
        await publisher.Publish(new ProductionOrderCompletedEvent(order.Id, order.OrderNumber, order.CompletedQuantity, order.ScrapQuantity), ct);

        return MapToResponse(order);
    }

    public async Task<ProductionOrderResponse> CancelAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status is not (ProductionOrderStatus.Draft or ProductionOrderStatus.Released or ProductionOrderStatus.OnHold))
            throw new BusinessRuleException("ORDER_CANNOT_CANCEL", $"Cannot cancel order with status {order.Status}.");

        order.Status = ProductionOrderStatus.Cancelled;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Production order {OrderNumber} cancelled. Reason: {Reason}", order.OrderNumber, reason);
        await publisher.Publish(new ProductionOrderCancelledEvent(order.Id, order.OrderNumber, reason), ct);

        return MapToResponse(order);
    }

    public async Task<ProductionOrderResponse> HoldAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status is not (ProductionOrderStatus.Released or ProductionOrderStatus.InProgress))
            throw new BusinessRuleException("ORDER_CANNOT_HOLD", $"Cannot put order with status {order.Status} on hold.");

        order.Status = ProductionOrderStatus.OnHold;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Production order {OrderNumber} put on hold", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task<ProductionOrderResponse> ResumeAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status != ProductionOrderStatus.OnHold)
            throw new BusinessRuleException("ORDER_NOT_ON_HOLD", "Only ON_HOLD orders can be resumed.");

        order.Status = ProductionOrderStatus.Released;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Production order {OrderNumber} resumed to Released", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);

        if (order.Status is not (ProductionOrderStatus.Draft or ProductionOrderStatus.Cancelled))
            throw new BusinessRuleException("ORDER_CANNOT_DELETE", "Only DRAFT or CANCELLED orders can be deleted.");

        await orderRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Production order {OrderNumber} soft-deleted", order.OrderNumber);
    }

    private static ProductionOrderResponse MapToResponse(ProductionOrder o) => new(
        o.Id,
        o.OrderNumber,
        o.ProductId,
        o.WorkCenterId,
        o.WorkCenter?.Name,
        o.Status.ToString(),
        o.PlannedQuantity,
        o.CompletedQuantity,
        o.ScrapQuantity,
        o.PlannedStartDate,
        o.PlannedEndDate,
        o.ActualStartDate,
        o.ActualEndDate,
        o.EstimatedCost,
        o.ActualCost,
        o.Notes,
        o.Lines.OrderBy(l => l.Position).Select(MapLineToResponse).ToList(),
        o.CreatedAt,
        o.UpdatedAt);

    private static ProductionOrderLineResponse MapLineToResponse(ProductionOrderLine l) => new(
        l.Id,
        l.MaterialId,
        l.Description,
        l.RequiredQuantity,
        l.IssuedQuantity,
        l.Position);
}
