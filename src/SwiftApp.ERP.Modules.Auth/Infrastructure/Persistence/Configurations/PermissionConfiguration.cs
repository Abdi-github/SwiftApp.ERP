using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : BaseEntityConfiguration<Permission>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("code");

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(e => e.Module)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("module");

        builder.HasIndex(e => e.Code).IsUnique();
    }
}
