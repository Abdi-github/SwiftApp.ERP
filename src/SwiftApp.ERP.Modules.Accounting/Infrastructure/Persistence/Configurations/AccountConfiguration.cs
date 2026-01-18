using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Accounting.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : BaseEntityConfiguration<Account>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.Property(e => e.AccountNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("account_number");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(e => e.AccountType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("account_type");

        builder.Property(e => e.ParentId)
            .HasColumnName("parent_id");

        builder.Property(e => e.Active)
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.Property(e => e.Balance)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("balance");

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AccountNumber).IsUnique();
    }
}
