using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Auth.Application.DTOs;
using SwiftApp.ERP.Modules.Auth.Application.Services;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.Modules.Auth.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Infrastructure;

/// <summary>
/// Implements IAuthModuleApi for cross-module queries.
/// </summary>
public class AuthModuleApiFacade(
    UserService userService,
    ICurrentUserService currentUserService,
    AppDbContext db) : IAuthModuleApi
{
    public async Task<UserResponse?> FindUserByIdAsync(Guid id, CancellationToken ct = default)
        => await userService.GetByIdAsync(id, ct);

    public async Task<UserResponse?> FindUserByUsernameAsync(string username, CancellationToken ct = default)
        => await userService.GetByUsernameAsync(username, ct);

    public string? GetCurrentUsername()
        => currentUserService.Username;

    public async Task<List<UserResponse>> FindAllEnabledUsersAsync(CancellationToken ct = default)
    {
        var result = await userService.GetPagedAsync(1, int.MaxValue, null, ct);
        return result.Items.Where(u => u.Enabled).ToList();
    }

    public async Task<List<UserResponse>> FindAllByRoleAsync(string roleName, CancellationToken ct = default)
    {
        var result = await userService.GetPagedAsync(1, int.MaxValue, null, ct);
        return result.Items.Where(u => u.Roles.Contains(roleName)).ToList();
    }

    public async Task<IReadOnlySet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Set<User>()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return new HashSet<string>().AsReadOnly();

        return user.Roles
            .SelectMany(r => r.Permissions)
            .Select(p => p.Code)
            .ToHashSet()
            .AsReadOnly();
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        var permissions = await GetPermissionsForUserAsync(userId, ct);
        return permissions.Contains(permissionCode);
    }
}
