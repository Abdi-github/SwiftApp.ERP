using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Auth.Domain.Entities;

/// <summary>
/// Application user entity — username/password authentication with role-based access.
/// Maps to Java: ch.swiftapp.erp.security.User
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public bool Locked { get; set; }

    public string DisplayName => $"{FirstName} {LastName}".Trim();

    // Navigation: many-to-many via user_roles join table
    public ICollection<Role> Roles { get; set; } = [];
}
