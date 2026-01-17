using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Production.Domain.Events;

public record ProductionOrderCreatedEvent(Guid OrderId, string OrderNumber, Guid ProductId) : IDomainEvent;

public record ProductionOrderReleasedEvent(Guid OrderId, string OrderNumber) : IDomainEvent;

public record ProductionOrderCompletedEvent(Guid OrderId, string OrderNumber, decimal CompletedQuantity, decimal ScrapQuantity) : IDomainEvent;

public record ProductionOrderCancelledEvent(Guid OrderId, string OrderNumber, string Reason) : IDomainEvent;
