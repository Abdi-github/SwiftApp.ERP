using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.SeedData;

/// <summary>
/// Seeds default roles, permissions, and an admin user.
/// Mirrors Java Flyway seed: 8 roles, ~50 permissions, 1 admin user.
/// </summary>
public static class AuthSeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Permission>().AnyAsync())
        {
            logger.LogInformation("Auth seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding Auth module data...");

        // ── Permissions ──
        var permissions = CreatePermissions();
        await db.Set<Permission>().AddRangeAsync(permissions);
        await db.SaveChangesAsync();

        var permByCode = permissions.ToDictionary(p => p.Code);

        // ── Roles ──
        var roles = CreateRoles(permByCode);
        await db.Set<Role>().AddRangeAsync(roles);
        await db.SaveChangesAsync();

        // ── Admin User ──
        var adminRole = roles.First(r => r.Name == "ADMIN");
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@swiftapp.ch",
            PasswordHash = HashPassword("Admin@123"),
            FirstName = "System",
            LastName = "Administrator",
            Enabled = true,
            Locked = false,
            Roles = { adminRole }
        };
        await db.Set<User>().AddAsync(adminUser);
        await db.SaveChangesAsync();

        logger.LogInformation("Auth seed data created: {PermCount} permissions, {RoleCount} roles, 1 admin user",
            permissions.Count, roles.Count);
    }

    private static List<Permission> CreatePermissions()
    {
        var modules = new Dictionary<string, string[]>
        {
            ["ADMIN"] = ["USERS_VIEW", "USERS_MANAGE", "ROLES_VIEW", "ROLES_MANAGE", "SETTINGS_VIEW", "SETTINGS_MANAGE"],
            ["MASTERDATA"] = ["VIEW", "CREATE", "EDIT", "DELETE"],
            ["INVENTORY"] = ["VIEW", "CREATE", "EDIT", "DELETE", "ADJUST"],
            ["SALES"] = ["VIEW", "CREATE", "EDIT", "DELETE", "APPROVE"],
            ["PURCHASING"] = ["VIEW", "CREATE", "EDIT", "DELETE", "APPROVE"],
            ["PRODUCTION"] = ["VIEW", "CREATE", "EDIT", "DELETE", "PLAN"],
            ["ACCOUNTING"] = ["VIEW", "CREATE", "EDIT", "DELETE", "CLOSE"],
            ["HR"] = ["VIEW", "CREATE", "EDIT", "DELETE"],
            ["CRM"] = ["VIEW", "CREATE", "EDIT", "DELETE"],
            ["QC"] = ["VIEW", "CREATE", "EDIT", "DELETE", "APPROVE"],
            ["NOTIFICATION"] = ["VIEW", "CREATE", "MANAGE"],
            ["DASHBOARD"] = ["VIEW"],
        };

        var permissions = new List<Permission>();
        foreach (var (module, actions) in modules)
        {
            foreach (var action in actions)
            {
                permissions.Add(new Permission
                {
                    Code = $"{module}:{action}",
                    Description = $"{action} permission for {module} module",
                    Module = module
                });
            }
        }

        return permissions;
    }

    private static List<Role> CreateRoles(Dictionary<string, Permission> permByCode)
    {
        List<Permission> All() => [.. permByCode.Values];
        List<Permission> ForModules(params string[] modules) =>
            [.. permByCode.Values.Where(p => modules.Contains(p.Module))];
        List<Permission> ViewAll() =>
            [.. permByCode.Values.Where(p => p.Code.EndsWith(":VIEW"))];
        // Combine multiple permission sets, deduplicating by Id to avoid EF tracking conflicts
        List<Permission> Combine(params List<Permission>[] sets) =>
            [.. sets.SelectMany(s => s).DistinctBy(p => p.Id)];

        return
        [
            new() { Name = "ADMIN", Description = "Full system administrator", Permissions = All() },
            new() { Name = "MANAGER", Description = "Department manager with broad read access", Permissions = Combine(ViewAll(), ForModules("DASHBOARD")) },
            new() { Name = "ACCOUNTANT", Description = "Accounting department", Permissions = ForModules("ACCOUNTING", "DASHBOARD") },
            new() { Name = "SALES", Description = "Sales department", Permissions = ForModules("SALES", "MASTERDATA", "INVENTORY", "CRM", "DASHBOARD") },
            new() { Name = "PRODUCTION", Description = "Production department", Permissions = ForModules("PRODUCTION", "MASTERDATA", "INVENTORY", "QC", "DASHBOARD") },
            new() { Name = "WAREHOUSE", Description = "Warehouse operations", Permissions = ForModules("INVENTORY", "MASTERDATA", "DASHBOARD") },
            new() { Name = "HR", Description = "Human Resources", Permissions = ForModules("HR", "DASHBOARD") },
            new() { Name = "VIEWER", Description = "Read-only access to all modules", Permissions = ViewAll() },
        ];
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}
