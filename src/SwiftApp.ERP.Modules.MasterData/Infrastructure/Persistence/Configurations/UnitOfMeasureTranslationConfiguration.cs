using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Configurations;

public class UnitOfMeasureTranslationConfiguration : BaseTranslationConfiguration<UnitOfMeasureTranslation>
{
    protected override void ConfigureTranslation(EntityTypeBuilder<UnitOfMeasureTranslation> builder)
    {
        builder.ToTable("uom_translations");

        builder.Property(e => e.UnitOfMeasureId)
            .IsRequired()
            .HasColumnName("uom_id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("description");

        builder.HasOne(e => e.UnitOfMeasure)
            .WithMany(u => u.Translations)
            .HasForeignKey(e => e.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UnitOfMeasureId, e.Locale }).IsUnique();
    }
}
