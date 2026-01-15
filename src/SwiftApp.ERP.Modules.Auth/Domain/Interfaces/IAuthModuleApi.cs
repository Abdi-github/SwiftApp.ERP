using SwiftApp.ERP.Modules.Auth.Application.DTOs;

namespace SwiftApp.ERP.Modules.Auth.Domain.Interfaces;

/// <summary>
/// Public API for the Auth module — consumed by other modules via DI.
/// Maps to Java: AuthModuleApi interface.
/// </summary>
public interface IAuthModuleApi
{
    Task<UserResponse?> FindUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserResponse?> FindUserByUsernameAsync(string username, CancellationToken ct = default);
    string? GetCurrentUsername();
    Task<List<UserResponse>> FindAllEnabledUsersAsync(CancellationToken ct = default);
    Task<List<UserResponse>> FindAllByRoleAsync(string roleName, CancellationToken ct = default);
    Task<IReadOnlySet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default);
}
