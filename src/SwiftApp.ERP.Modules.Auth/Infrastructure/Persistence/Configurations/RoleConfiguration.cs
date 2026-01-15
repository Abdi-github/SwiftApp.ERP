using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : BaseEntityConfiguration<Role>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.HasIndex(e => e.Name).IsUnique();

        // Many-to-many: Role <-> Permission via role_permissions join table
        builder.HasMany(e => e.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity("role_permissions",
                l => l.HasOne(typeof(Permission)).WithMany().HasForeignKey("permission_id"),
                r => r.HasOne(typeof(Role)).WithMany().HasForeignKey("role_id"),
                j =>
                {
                    j.HasKey("role_id", "permission_id");
                    j.ToTable("role_permissions");
                });
    }
}
