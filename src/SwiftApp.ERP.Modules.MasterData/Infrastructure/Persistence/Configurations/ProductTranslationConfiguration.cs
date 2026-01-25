using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Configurations;

public class ProductTranslationConfiguration : BaseTranslationConfiguration<ProductTranslation>
{
    protected override void ConfigureTranslation(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("product_translations");

        builder.Property(e => e.ProductId)
            .IsRequired()
            .HasColumnName("product_id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.HasOne(e => e.Product)
            .WithMany(p => p.Translations)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ProductId, e.Locale }).IsUnique();
    }
}
