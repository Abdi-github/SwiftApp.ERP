using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Purchasing.Domain.Events;

public record PurchaseOrderCreatedEvent(Guid OrderId, string OrderNumber, Guid SupplierId) : IDomainEvent;

public record PurchaseOrderConfirmedEvent(Guid OrderId, string OrderNumber, decimal TotalAmount) : IDomainEvent;

public record PurchaseOrderReceivedEvent(Guid OrderId, string OrderNumber) : IDomainEvent;

public record PurchaseOrderCancelledEvent(Guid OrderId, string OrderNumber, string Reason) : IDomainEvent;
