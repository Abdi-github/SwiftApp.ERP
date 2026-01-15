namespace SwiftApp.ERP.Modules.Auth.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record JwtResponse(string Token, string Type, string Username);

public record UserRequest(
    string Username,
    string Email,
    string? Password,
    string FirstName,
    string LastName,
    bool? Enabled,
    IReadOnlySet<string>? RoleNames);

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    bool Enabled,
    bool Locked,
    IReadOnlySet<string> Roles,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record RoleRequest(
    string Name,
    string? Description,
    IReadOnlySet<Guid>? PermissionIds);

public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlySet<PermissionResponse> Permissions);

public record PermissionResponse(
    Guid Id,
    string Code,
    string? Description,
    string Module);
