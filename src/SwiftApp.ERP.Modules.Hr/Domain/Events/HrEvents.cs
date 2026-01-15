using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Hr.Domain.Events;

public record EmployeeHiredEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    string Name,
    Guid DepartmentId) : IDomainEvent;

public record EmployeeTerminatedEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    DateOnly TerminationDate) : IDomainEvent;
