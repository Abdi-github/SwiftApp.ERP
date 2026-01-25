using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.QualityControl.Domain.Events;

public record QualityCheckCompletedEvent(
    Guid CheckId,
    string CheckNumber,
    string Result) : IDomainEvent;

public record NonConformanceReportCreatedEvent(
    Guid NcrId,
    string NcrNumber,
    string Severity) : IDomainEvent;

public record NonConformanceReportClosedEvent(
    Guid NcrId,
    string NcrNumber) : IDomainEvent;
