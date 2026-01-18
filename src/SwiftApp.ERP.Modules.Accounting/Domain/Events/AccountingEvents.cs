using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Accounting.Domain.Events;

public record JournalEntryPostedEvent(Guid EntryId, string EntryNumber, decimal TotalAmount) : IDomainEvent;

public record JournalEntryReversedEvent(
    Guid OriginalEntryId,
    Guid ReversalEntryId,
    string OriginalEntryNumber,
    string ReversalEntryNumber) : IDomainEvent;
