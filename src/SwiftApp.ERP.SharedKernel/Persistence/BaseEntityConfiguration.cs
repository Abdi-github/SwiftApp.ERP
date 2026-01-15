using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.SharedKernel.Persistence;

/// <summary>
/// Base EF Core configuration for all entities that extend BaseEntity.
/// Configures Id, audit fields, soft delete, and xmin concurrency token.
/// </summary>
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256)
            .HasColumnName("created_by");

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256)
            .HasColumnName("updated_by");

        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at");

        // PostgreSQL xmin concurrency token (auto-detected by Npgsql when uint + ConcurrencyToken + OnAddOrUpdate)
        builder.Property(e => e.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.Ignore(e => e.IsDeleted);

        ConfigureEntity(builder);
    }

    /// <summary>
    /// Override in derived classes to add entity-specific configuration.
    /// </summary>
    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}
