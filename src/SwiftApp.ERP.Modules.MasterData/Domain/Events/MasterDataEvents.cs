using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Events;

public record ProductCreatedEvent(Guid ProductId, string Sku, string Name) : IDomainEvent;

public record ProductUpdatedEvent(Guid ProductId, string Sku) : IDomainEvent;

public record ProductDeletedEvent(Guid ProductId, string Sku) : IDomainEvent;
