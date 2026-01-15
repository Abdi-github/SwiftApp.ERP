using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Auth.Domain.Entities;

/// <summary>
/// RBAC role entity — groups permissions for users.
/// Maps to Java: ch.swiftapp.erp.security.Role
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation: many-to-many via role_permissions join table
    public ICollection<Permission> Permissions { get; set; } = [];

    // Navigation: many-to-many via user_roles join table
    public ICollection<User> Users { get; set; } = [];
}
