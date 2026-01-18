using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Sales.Application.DTOs;
using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Enums;
using SwiftApp.ERP.Modules.Sales.Domain.Events;
using SwiftApp.ERP.Modules.Sales.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Sales.Application.Services;

public class SalesOrderService(
    ISalesOrderRepository orderRepository,
    ISalesOrderLineRepository lineRepository,
    ICustomerRepository customerRepository,
    IPublisher publisher,
    ILogger<SalesOrderService> logger)
{
    public async Task<SalesOrderResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<SalesOrderResponse?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByOrderNumberAsync(orderNumber, ct);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<PagedResult<SalesOrderResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await orderRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<SalesOrderResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<PagedResult<SalesOrderResponse>> GetByCustomerAsync(Guid customerId, int page, int size, CancellationToken ct = default)
    {
        var result = await orderRepository.GetByCustomerAsync(customerId, page, size, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<SalesOrderResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<SalesOrderResponse> CreateAsync(SalesOrderRequest request, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, ct)
            ?? throw new EntityNotFoundException(nameof(Customer), request.CustomerId);

        var year = DateTime.UtcNow.Year;
        var seq = await orderRepository.GetMaxSequenceForYearAsync(year, ct) + 1;
        var orderNumber = $"SO-{year}-{seq:D5}";
        // TODO: Optimize sequence generation with Redis counter to reduce DB load

        var order = new SalesOrder
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            Customer = customer,
            OrderDate = request.OrderDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            DeliveryDate = request.DeliveryDate,
            Notes = request.Notes,
            ShippingStreet = request.ShippingStreet ?? customer.Street,
            ShippingCity = request.ShippingCity ?? customer.City,
            ShippingPostalCode = request.ShippingPostalCode ?? customer.PostalCode,
            ShippingCanton = request.ShippingCanton ?? customer.Canton,
            ShippingCountry = request.ShippingCountry ?? customer.Country,
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
        // System.Diagnostics.Debug.WriteLine($"Recalculated totals for {order.OrderNumber}: subtotal={order.Subtotal}, vat={order.VatAmount}, total={order.TotalAmount}");
        await orderRepository.AddAsync(order, ct);

        logger.LogInformation("Sales order {OrderNumber} created for customer {CustomerId}", order.OrderNumber, order.CustomerId);
        await publisher.Publish(new SalesOrderCreatedEvent(order.Id, order.OrderNumber, order.CustomerId), ct);

        return MapToResponse(order);
    }

    public async Task<SalesOrderResponse> UpdateAsync(Guid id, SalesOrderRequest request, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(SalesOrder), id);

        if (order.Status != SalesOrderStatus.Draft)
            throw new BusinessRuleException("ORDER_NOT_DRAFT", "Only DRAFT orders can be updated.");

        var customer = await customerRepository.GetByIdAsync(request.CustomerId, ct)
            ?? throw new EntityNotFoundException(nameof(Customer), request.CustomerId);

        order.CustomerId = request.CustomerId;
        order.Customer = customer;
        order.OrderDate = request.OrderDate ?? order.OrderDate;
        order.DeliveryDate = request.DeliveryDate;
        order.Notes = request.Notes;
        order.ShippingStreet = request.ShippingStreet ?? customer.Street;
        order.ShippingCity = request.ShippingCity ?? customer.City;
        order.ShippingPostalCode = request.ShippingPostalCode ?? customer.PostalCode;
        order.ShippingCanton = request.ShippingCanton ?? customer.Canton;
        order.ShippingCountry = request.ShippingCountry ?? customer.Country;

        // Replace all lines
        await lineRepository.DeleteByOrderIdAsync(id, ct);
        order.Lines.Clear();

        if (request.Lines is { Count: > 0 })
        {
            var position = 0;
            foreach (var lineReq in request.Lines)
            {
                var line = MapToLine(lineReq, position++);
                line.SalesOrderId = id;
                line.CalculateLineTotal();
                order.Lines.Add(line);
            }
        }

        order.RecalculateTotals();
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Sales order {OrderNumber} updated", order.OrderNumber);

        return MapToResponse(order);
    }

    public async Task<SalesOrderResponse> ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(SalesOrder), id);

        if (order.Status != SalesOrderStatus.Draft)
            throw new BusinessRuleException("ORDER_NOT_DRAFT", "Only DRAFT orders can be confirmed.");

        if (order.Lines.Count == 0)
            throw new BusinessRuleException("ORDER_NO_LINES", "Order must have at least one line to be confirmed.");

        order.Status = SalesOrderStatus.Confirmed;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Sales order {OrderNumber} confirmed with total {TotalAmount}", order.OrderNumber, order.TotalAmount);
        await publisher.Publish(new SalesOrderConfirmedEvent(order.Id, order.OrderNumber, order.TotalAmount), ct);

        return MapToResponse(order);
    }

    public async Task<SalesOrderResponse> AdvanceStatusAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(SalesOrder), id);

        order.Status = order.Status switch
        {
            SalesOrderStatus.Confirmed => SalesOrderStatus.Processing,
            SalesOrderStatus.Processing => SalesOrderStatus.Shipped,
            SalesOrderStatus.Shipped => SalesOrderStatus.Delivered,
            SalesOrderStatus.Delivered => SalesOrderStatus.Completed,
            _ => throw new BusinessRuleException("INVALID_STATUS_TRANSITION", $"Cannot advance status from {order.Status}.")
        };
        // logger.LogDebug("Order status advanced for {OrderNumber} to {Status}", order.OrderNumber, order.Status);

        await orderRepository.UpdateAsync(order, ct);
        logger.LogInformation("Sales order {OrderNumber} advanced to {Status}", order.OrderNumber, order.Status);

        return MapToResponse(order);
    }

    public async Task<SalesOrderResponse> CancelAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(SalesOrder), id);

        if (order.Status is SalesOrderStatus.Completed or SalesOrderStatus.Cancelled)
            throw new BusinessRuleException("ORDER_CANNOT_CANCEL", $"Cannot cancel order with status {order.Status}.");

        order.Status = SalesOrderStatus.Cancelled;
        await orderRepository.UpdateAsync(order, ct);

        logger.LogInformation("Sales order {OrderNumber} cancelled. Reason: {Reason}", order.OrderNumber, reason);
        await publisher.Publish(new SalesOrderCancelledEvent(order.Id, order.OrderNumber, reason), ct);

        return MapToResponse(order);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(SalesOrder), id);

        if (order.Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Cancelled))
            throw new BusinessRuleException("ORDER_CANNOT_DELETE", "Only DRAFT or CANCELLED orders can be deleted.");

        await orderRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Sales order {OrderNumber} soft-deleted", order.OrderNumber);
    }

    private static SalesOrderLine MapToLine(SalesOrderLineRequest req, int defaultPosition)
    {
        var vatRate = Enum.Parse<VatRate>(req.VatRate, ignoreCase: true);
        return new SalesOrderLine
        {
            ProductId = req.ProductId,
            Description = req.Description,
            Quantity = req.Quantity,
            UnitPrice = req.UnitPrice,
            DiscountPct = req.DiscountPct ?? 0m,
            VatRate = vatRate,
            Position = req.Position ?? defaultPosition,
        };
    }

    private static SalesOrderResponse MapToResponse(SalesOrder o) => new(
        o.Id,
        o.OrderNumber,
        o.CustomerId,
        o.Customer?.DisplayName ?? string.Empty,
        o.Status.ToString(),
        o.OrderDate,
        o.DeliveryDate,
        o.Subtotal,
        o.VatAmount,
        o.TotalAmount,
        o.Currency,
        o.Notes,
        o.ShippingStreet,
        o.ShippingCity,
        o.ShippingPostalCode,
        o.ShippingCanton,
        o.ShippingCountry,
        o.Lines.OrderBy(l => l.Position).Select(MapLineToResponse).ToList(),
        o.CreatedAt,
        o.UpdatedAt);

    private static SalesOrderLineResponse MapLineToResponse(SalesOrderLine l) => new(
        l.Id,
        l.ProductId,
        l.Description,
        l.Quantity,
        l.UnitPrice,
        l.DiscountPct,
        l.VatRate.ToString(),
        l.LineTotal,
        l.GetVatAmount(),
        l.Position);
}
