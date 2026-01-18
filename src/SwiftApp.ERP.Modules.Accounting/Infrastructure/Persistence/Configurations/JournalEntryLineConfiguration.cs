using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Accounting.Infrastructure.Persistence.Configurations;

public class JournalEntryLineConfiguration : BaseEntityConfiguration<JournalEntryLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("journal_entry_lines");

        builder.Property(e => e.JournalEntryId)
            .IsRequired()
            .HasColumnName("journal_entry_id");

        builder.Property(e => e.AccountId)
            .IsRequired()
            .HasColumnName("account_id");

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(e => e.Debit)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("debit");

        builder.Property(e => e.Credit)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("credit");

        builder.Property(e => e.Position)
            .HasDefaultValue(0)
            .HasColumnName("position");

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
