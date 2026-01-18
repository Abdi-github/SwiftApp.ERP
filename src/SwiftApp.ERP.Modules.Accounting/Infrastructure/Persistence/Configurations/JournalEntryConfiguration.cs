using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Accounting.Infrastructure.Persistence.Configurations;

public class JournalEntryConfiguration : BaseEntityConfiguration<JournalEntry>
{
    protected override void ConfigureEntity(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");

        builder.Property(e => e.EntryNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("entry_number");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(e => e.EntryDate)
            .IsRequired()
            .HasColumnName("entry_date");

        builder.Property(e => e.Posted)
            .HasDefaultValue(false)
            .HasColumnName("posted");

        builder.Property(e => e.Reversed)
            .HasDefaultValue(false)
            .HasColumnName("reversed");

        builder.Property(e => e.Reference)
            .HasMaxLength(255)
            .HasColumnName("reference");

        builder.Property(e => e.SourceDocumentType)
            .HasMaxLength(50)
            .HasColumnName("source_document_type");

        builder.Property(e => e.SourceDocumentId)
            .HasColumnName("source_document_id");

        builder.HasMany(e => e.Lines)
            .WithOne(l => l.JournalEntry)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.EntryNumber).IsUnique();
    }
}
