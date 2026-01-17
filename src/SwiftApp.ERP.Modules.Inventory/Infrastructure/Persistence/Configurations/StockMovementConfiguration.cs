using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : BaseEntityConfiguration<StockMovement>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.Property(e => e.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("reference_number");

        builder.Property(e => e.MovementType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnName("movement_type");

        builder.Property(e => e.ItemId)
            .IsRequired()
            .HasColumnName("item_id");

        builder.Property(e => e.ItemType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnName("item_type");

        builder.Property(e => e.SourceWarehouseId)
            .HasColumnName("source_warehouse_id");

        builder.Property(e => e.TargetWarehouseId)
            .HasColumnName("target_warehouse_id");

        builder.Property(e => e.Quantity)
            .HasPrecision(19, 4)
            .HasColumnName("quantity");

        builder.Property(e => e.MovementDate)
            .IsRequired()
            .HasColumnName("movement_date");

        builder.Property(e => e.Reason)
            .HasMaxLength(500)
            .HasColumnName("reason");

        builder.Property(e => e.SourceDocumentType)
            .HasMaxLength(50)
            .HasColumnName("source_document_type");

        builder.Property(e => e.SourceDocumentId)
            .HasColumnName("source_document_id");

        builder.HasOne(e => e.SourceWarehouse)
            .WithMany()
            .HasForeignKey(e => e.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetWarehouse)
            .WithMany()
            .HasForeignKey(e => e.TargetWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ReferenceNumber).IsUnique();
    }
}
