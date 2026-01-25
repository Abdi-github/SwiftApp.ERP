using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Configurations;

public class MaterialTranslationConfiguration : BaseTranslationConfiguration<MaterialTranslation>
{
    protected override void ConfigureTranslation(EntityTypeBuilder<MaterialTranslation> builder)
    {
        builder.ToTable("material_translations");

        builder.Property(e => e.MaterialId)
            .IsRequired()
            .HasColumnName("material_id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.HasOne(e => e.Material)
            .WithMany(m => m.Translations)
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.MaterialId, e.Locale }).IsUnique();
    }
}
