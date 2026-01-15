using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Interfaces;

namespace SwiftApp.ERP.SharedKernel.Persistence;

/// <summary>
/// Central DbContext for the entire ERP. Each module registers its entity configurations
/// via IEntityTypeConfiguration discovered from referenced assemblies.
/// </summary>
public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService? currentUserService = null) : DbContext(options)
{
    private readonly ICurrentUserService? _currentUserService = currentUserService;

    /// <summary>
    /// Assemblies whose IEntityTypeConfiguration classes should be scanned.
    /// Host projects register module assemblies here at startup.
    /// </summary>
    public static List<Assembly> ConfigurationAssemblies { get; } = [];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from all registered module assemblies
        foreach (var assembly in ConfigurationAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        // Global query filter for soft delete on all BaseEntity subtypes
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(
                        BuildSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTimeOffset.UtcNow;
        var username = _currentUserService?.Username ?? "system";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = username;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = username;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseTranslation>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = username;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = username;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Builds a lambda expression: entity => ((BaseEntity)entity).DeletedAt == null
    /// </summary>
    private static System.Linq.Expressions.LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var deletedAtProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
        var nullConstant = System.Linq.Expressions.Expression.Constant(null, typeof(DateTimeOffset?));
        var comparison = System.Linq.Expressions.Expression.Equal(deletedAtProperty, nullConstant);
        return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
    }
}
