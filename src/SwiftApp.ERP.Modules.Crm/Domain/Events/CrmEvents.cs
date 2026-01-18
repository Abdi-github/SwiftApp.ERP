using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Crm.Domain.Events;

public record ContactCreatedEvent(Guid ContactId, string Name) : IDomainEvent;

public record InteractionCreatedEvent(Guid InteractionId, Guid ContactId, string InteractionType) : IDomainEvent;
