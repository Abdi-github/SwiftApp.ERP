using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Configurations;

public class CategoryTranslationConfiguration : BaseTranslationConfiguration<CategoryTranslation>
{
    protected override void ConfigureTranslation(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable("category_translations");

        builder.Property(e => e.CategoryId)
            .IsRequired()
            .HasColumnName("category_id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .HasColumnName("description");

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Translations)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CategoryId, e.Locale }).IsUnique();
    }
}
