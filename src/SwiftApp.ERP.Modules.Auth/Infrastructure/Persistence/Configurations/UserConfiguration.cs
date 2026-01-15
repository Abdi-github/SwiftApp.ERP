using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("username");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnName("email");

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasColumnName("password_hash");

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("last_name");

        builder.Property(e => e.Enabled)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("enabled");

        builder.Property(e => e.Locked)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("locked");

        builder.Ignore(e => e.DisplayName);

        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();

        // Many-to-many: User <-> Role via user_roles join table
        builder.HasMany(e => e.Roles)
            .WithMany(r => r.Users)
            .UsingEntity("user_roles",
                l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("role_id"),
                r => r.HasOne(typeof(User)).WithMany().HasForeignKey("user_id"),
                j =>
                {
                    j.HasKey("user_id", "role_id");
                    j.ToTable("user_roles");
                });
    }
}
