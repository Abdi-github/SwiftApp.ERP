using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Auth.Domain.Entities;

/// <summary>
/// Permission entity — fine-grained access control code in MODULE:ACTION format.
/// Maps to Java: ch.swiftapp.erp.security.Permission
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Unique permission code, e.g. "SALES:VIEW", "ADMIN:USERS_MANAGE".
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Module this permission belongs to, e.g. "SALES", "ADMIN".
    /// </summary>
    public string Module { get; set; } = string.Empty;

    // Navigation: many-to-many via role_permissions join table
    public ICollection<Role> Roles { get; set; } = [];
}
