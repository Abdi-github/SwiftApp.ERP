using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Auth.Application.DTOs;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.Modules.Auth.Domain.Events;
using SwiftApp.ERP.Modules.Auth.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Application.Services;

/// <summary>
/// Business logic for role management.
/// Maps to Java: RoleService — CRUD, permission assignment, prevents ADMIN deletion.
/// </summary>
public class RoleService(
    IRoleRepository roleRepository,
    AppDbContext db,
    IPublisher publisher,
    ILogger<RoleService> logger)
{
    public async Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var role = await roleRepository.GetByIdAsync(id, ct);
        return role is null ? null : MapToResponse(role);
    }

    public async Task<List<RoleResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var roles = await roleRepository.GetAllAsync(ct);
        return roles.Select(MapToResponse).ToList();
    }

    public async Task<Dictionary<string, List<PermissionResponse>>> GetPermissionsGroupedByModuleAsync(CancellationToken ct = default)
    {
        var permissions = await db.Set<Permission>()
            .OrderBy(p => p.Module).ThenBy(p => p.Code)
            .ToListAsync(ct);

        return permissions
            .GroupBy(p => p.Module)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => new PermissionResponse(p.Id, p.Code, p.Description, p.Module)).ToList());
    }

    public async Task<RoleResponse> CreateAsync(RoleRequest request, CancellationToken ct = default)
    {
        if (await roleRepository.GetByNameAsync(request.Name, ct) is not null)
            throw new BusinessRuleException("UNIQUE_ROLE", $"Role '{request.Name}' already exists.");

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
        };

        if (request.PermissionIds is { Count: > 0 })
        {
            var permissions = await db.Set<Permission>()
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(ct);
            foreach (var perm in permissions)
                role.Permissions.Add(perm);
        }

        await roleRepository.AddAsync(role, ct);
        logger.LogInformation("Role {RoleName} created", role.Name);

        return MapToResponse(role);
    }

    public async Task<RoleResponse> UpdateAsync(Guid id, RoleRequest request, CancellationToken ct = default)
    {
        var role = await roleRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Role), id);

        if (role.Name != request.Name && await roleRepository.GetByNameAsync(request.Name, ct) is not null)
            throw new BusinessRuleException("UNIQUE_ROLE", $"Role '{request.Name}' already exists.");

        role.Name = request.Name;
        role.Description = request.Description;

        if (request.PermissionIds is not null)
        {
            role.Permissions.Clear();
            var permissions = await db.Set<Permission>()
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(ct);
            foreach (var perm in permissions)
                role.Permissions.Add(perm);
        }

        await roleRepository.UpdateAsync(role, ct);
        logger.LogInformation("Role {RoleName} updated", role.Name);

        var permCodes = role.Permissions.Select(p => p.Code).ToHashSet();
        await publisher.Publish(
            new RolePermissionsChangedEvent(role.Id, role.Name, permCodes.AsReadOnly()), ct);

        return MapToResponse(role);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await roleRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Role), id);

        if (role.Name is "ADMIN")
            throw new BusinessRuleException("ADMIN_UNDELETABLE", "The ADMIN role cannot be deleted.");

        await roleRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Role {RoleName} soft-deleted", role.Name);
    }

    private static RoleResponse MapToResponse(Role role) => new(
        role.Id,
        role.Name,
        role.Description,
        role.Permissions
            .Select(p => new PermissionResponse(p.Id, p.Code, p.Description, p.Module))
            .ToHashSet()
            .AsReadOnly());
}
