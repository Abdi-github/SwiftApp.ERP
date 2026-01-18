using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Purchasing.Application.DTOs;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.Modules.Purchasing.Domain.Enums;
using SwiftApp.ERP.Modules.Purchasing.Domain.Events;
using SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Purchasing.Application.Services;

public class PurchaseOrderService(
    IPurchaseOrderRepository orderRepository,
    IPurchaseOrderLineRepository lineRepository,
    ISupplierRepository supplierRepository,
    IPublisher publisher,
    ILogger<PurchaseOrderService> logger)
{
    public async Task<PurchaseOrderResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<PurchaseOrderResponse?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByOrderNumberAsync(orderNumber, ct);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<PagedResult<PurchaseOrderResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await orderRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<PurchaseOrderResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<PagedResult<PurchaseOrderResponse>> GetBySupplierAsync(Guid supplierId, int page, int size, CancellationToken ct = default)
    {
        var result = await orderRepository.GetBySupplierAsync(supplierId, page, size, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<PurchaseOrderResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<PurchaseOrderResponse> CreateAsync(PurchaseOrderRequest request, CancellationToken ct = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, ct)
            ?? throw new EntityNotFoundException(nameof(Supplier), request.SupplierId);

        var year = DateTime.UtcNow.Year;
        var seq = await orderRepository.GetMaxSequenceForYearAsync(year, ct) + 1;
        var orderNumber = $"PO-{year}-{seq:D5}";

        var order = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = request.SupplierId,
            Supplier = supplier,
            OrderDate = request.OrderDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes,
        };

        if (request.Lines is { Count: > 0 })
        {
            var position = 0;
            foreach (var lineReq in request.Lines)
            {
                var line = MapToLine(lineReq, position++);
                line.CalculateLineTotal();
                order.Lines.Add(line);
            }
        }

        order.RecalculateTotals();
        // System.Diagnostics.Debug.WriteLine($"Purchase totals for {order.OrderNumber}: subtotal={order.Subtotal}, vat={order.VatAmount}, total={order.TotalAmount}");
        await orderRepository.AddAsync(order, ct);

        logger.LogInformation("Purchase order {OrderNumber} created for supplier {SupplierId}", order.OrderNumber, order.SupplierId);
        await publisher.Publish(new PurchaseOrderCreatedEvent(order.Id, order.OrderNumber, order.SupplierId), ct);

        return MapToResponse(order);
    }

    public async Task<PurchaseOrderResponse> UpdateAsync(Guid id, PurchaseOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);

        if (order.Status != PurchaseOrderStatus.Draft)
            throw new BusinessRuleException("ORDER_NOT_DRAFT", "Only DRAFT orders can be updated.");

        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, ct)
            ?? throw new EntityNotFoundException(nameof(Supplier), request.SupplierId);

        order.SupplierId = request.SupplierId;
        order.Supplier = supplier;
        order.OrderDate = request.OrderDate ?? order.OrderDate;
        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        order.Notes = request.Notes;

        // Replace all lines
        await lineRepository.DeleteByOrderIdAsync(id, ct);
        order.Lines.Clear();

        if (request.Lines is { Count: > 0 })
        {
            var position = 0;
            foreach (var lineReq in request.Lines)
            {
                var line = MapToLine(lineReq, position++);
                line.PurchaseOrderId = id;
                line.CalculateLineTotal();
                order.Lines.Add(line);
            }
        }

        order.RecalculateTotals();
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Purchase order {OrderNumber} updated", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task<PurchaseOrderResponse> SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);

        if (order.Status != PurchaseOrderStatus.Draft)
            throw new BusinessRuleException("ORDER_NOT_DRAFT", "Only DRAFT orders can be submitted.");

        if (order.Lines.Count == 0)
            throw new BusinessRuleException("ORDER_NO_LINES", "Order must have at least one line to be submitted.");

        order.Status = PurchaseOrderStatus.Submitted;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Purchase order {OrderNumber} submitted", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task<PurchaseOrderResponse> ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);

        if (order.Status != PurchaseOrderStatus.Submitted)
            throw new BusinessRuleException("ORDER_NOT_SUBMITTED", "Only SUBMITTED orders can be confirmed.");

        order.Status = PurchaseOrderStatus.Confirmed;
        // logger.LogDebug("Purchase order confirm transition for {OrderNumber}: status={Status}", order.OrderNumber, order.Status);
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Purchase order {OrderNumber} confirmed with total {TotalAmount}", order.OrderNumber, order.TotalAmount);
        await publisher.Publish(new PurchaseOrderConfirmedEvent(order.Id, order.OrderNumber, order.TotalAmount), ct);

        return MapToResponse(order);
    }

    public async Task<PurchaseOrderResponse> ReceiveAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);

        if (order.Status is not (PurchaseOrderStatus.Confirmed or PurchaseOrderStatus.PartiallyReceived))
            throw new BusinessRuleException("ORDER_CANNOT_RECEIVE", "Only CONFIRMED or PARTIALLY_RECEIVED orders can be received.");

        order.Status = PurchaseOrderStatus.Received;
        order.ActualDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Purchase order {OrderNumber} received", order.OrderNumber);
        await publisher.Publish(new PurchaseOrderReceivedEvent(order.Id, order.OrderNumber), ct);

        return MapToResponse(order);
    }

    public async Task<PurchaseOrderResponse> CompleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);

        if (order.Status != PurchaseOrderStatus.Received)
            throw new BusinessRuleException("ORDER_NOT_RECEIVED", "Only RECEIVED orders can be completed.");

        order.Status = PurchaseOrderStatus.Completed;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Purchase order {OrderNumber} completed", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task<PurchaseOrderResponse> CancelAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);

        if (order.Status is PurchaseOrderStatus.Completed or PurchaseOrderStatus.Cancelled)
            throw new BusinessRuleException("ORDER_CANNOT_CANCEL", $"Cannot cancel order with status {order.Status}.");

        order.Status = PurchaseOrderStatus.Cancelled;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Purchase order {OrderNumber} cancelled. Reason: {Reason}", order.OrderNumber, reason);
        await publisher.Publish(new PurchaseOrderCancelledEvent(order.Id, order.OrderNumber, reason), ct);

        return MapToResponse(order);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);

        if (order.Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.Cancelled))
            throw new BusinessRuleException("ORDER_CANNOT_DELETE", "Only DRAFT or CANCELLED orders can be deleted.");

        await orderRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Purchase order {OrderNumber} soft-deleted", order.OrderNumber);
    }

    private static PurchaseOrderLine MapToLine(PurchaseOrderLineRequest req, int defaultPosition)
    {
        var vatRate = Enum.Parse<VatRate>(req.VatRate, ignoreCase: true);
        return new PurchaseOrderLine
        {
            MaterialId = req.MaterialId,
            Description = req.Description,
            Quantity = req.Quantity,
            UnitPrice = req.UnitPrice,
            DiscountPct = req.DiscountPct ?? 0m,
            VatRate = vatRate,
            Position = req.Position ?? defaultPosition,
        };
    }

    private static PurchaseOrderResponse MapToResponse(PurchaseOrder o) => new(
        o.Id,
        o.OrderNumber,
        o.SupplierId,
        o.Supplier?.DisplayName ?? string.Empty,
        o.Status.ToString(),
        o.OrderDate,
        o.ExpectedDeliveryDate,
        o.ActualDeliveryDate,
        o.Subtotal,
        o.VatAmount,
        o.TotalAmount,
        o.Currency,
        o.Notes,
        o.Lines.OrderBy(l => l.Position).Select(MapLineToResponse).ToList(),
        o.CreatedAt,
        o.UpdatedAt);

    private static PurchaseOrderLineResponse MapLineToResponse(PurchaseOrderLine l) => new(
        l.Id,
        l.MaterialId,
        l.Description,
        l.Quantity,
        l.UnitPrice,
        l.DiscountPct,
        l.VatRate.ToString(),
        l.LineTotal,
        l.GetVatAmount(),
        l.Position);
}
