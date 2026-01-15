using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Auth.Domain.Events;

public record UserCreatedEvent(Guid UserId, string Username, string Email) : IDomainEvent;

public record UserRoleChangedEvent(Guid UserId, string Username, IReadOnlySet<string> NewRoles) : IDomainEvent;

public record RolePermissionsChangedEvent(Guid RoleId, string RoleName, IReadOnlySet<string> PermissionCodes) : IDomainEvent;
