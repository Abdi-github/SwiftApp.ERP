using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Sales.Domain.Events;

public record SalesOrderCreatedEvent(Guid OrderId, string OrderNumber, Guid CustomerId) : IDomainEvent;

public record SalesOrderConfirmedEvent(Guid OrderId, string OrderNumber, decimal TotalAmount) : IDomainEvent;

public record SalesOrderCancelledEvent(Guid OrderId, string OrderNumber, string Reason) : IDomainEvent;
