using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.Property(e => e.Sku)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("sku");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id");

        builder.Property(e => e.UnitPrice)
            .HasPrecision(19, 4)
            .HasColumnName("unit_price");

        builder.Property(e => e.ListPrice)
            .HasPrecision(19, 4)
            .HasColumnName("list_price");

        builder.Property(e => e.VatRate)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("vat_rate");

        builder.Property(e => e.Active)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Translations)
            .WithOne(t => t.Product)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Sku).IsUnique();
    }
}
