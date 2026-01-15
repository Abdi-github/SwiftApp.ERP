using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.SharedKernel.Persistence;

/// <summary>
/// Base EF Core configuration for all translation entities.
/// </summary>
public abstract class BaseTranslationConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseTranslation
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Locale)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("locale");

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

        // PostgreSQL xmin concurrency token
        builder.Property(e => e.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        ConfigureTranslation(builder);
    }

    /// <summary>
    /// Override in derived classes to add translation-specific configuration.
    /// </summary>
    protected abstract void ConfigureTranslation(EntityTypeBuilder<T> builder);
}
