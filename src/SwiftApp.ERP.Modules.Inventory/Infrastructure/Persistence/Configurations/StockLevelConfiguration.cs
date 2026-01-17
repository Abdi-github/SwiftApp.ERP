using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

public class StockLevelConfiguration : BaseEntityConfiguration<StockLevel>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("stock_levels");

        builder.Property(e => e.ItemId)
            .IsRequired()
            .HasColumnName("item_id");

        builder.Property(e => e.ItemType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnName("item_type");

        builder.Property(e => e.WarehouseId)
            .IsRequired()
            .HasColumnName("warehouse_id");

        builder.Property(e => e.QuantityOnHand)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("quantity_on_hand");

        builder.Property(e => e.QuantityReserved)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("quantity_reserved");

        builder.Ignore(e => e.QuantityAvailable);

        builder.HasOne(e => e.Warehouse)
            .WithMany()
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.ItemId, e.ItemType, e.WarehouseId }).IsUnique();
    }
}
