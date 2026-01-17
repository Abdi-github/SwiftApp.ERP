using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

public class WarehouseTranslationConfiguration : BaseTranslationConfiguration<WarehouseTranslation>
{
    protected override void ConfigureTranslation(EntityTypeBuilder<WarehouseTranslation> builder)
    {
        builder.ToTable("warehouse_translations");

        builder.Property(e => e.WarehouseId)
            .IsRequired()
            .HasColumnName("warehouse_id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.HasOne(e => e.Warehouse)
            .WithMany(w => w.Translations)
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.WarehouseId, e.Locale }).IsUnique();
    }
}
