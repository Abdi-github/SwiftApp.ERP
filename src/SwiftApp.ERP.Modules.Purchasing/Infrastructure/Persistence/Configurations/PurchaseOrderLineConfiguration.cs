using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public class PurchaseOrderLineConfiguration : BaseEntityConfiguration<PurchaseOrderLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("purchase_order_lines");

        builder.Property(e => e.PurchaseOrderId)
            .IsRequired()
            .HasColumnName("purchase_order_id");

        builder.Property(e => e.MaterialId)
            .IsRequired()
            .HasColumnName("material_id");

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(e => e.Quantity)
            .HasPrecision(19, 4)
            .HasColumnName("quantity");

        builder.Property(e => e.UnitPrice)
            .HasPrecision(19, 4)
            .HasColumnName("unit_price");

        builder.Property(e => e.DiscountPct)
            .HasPrecision(5, 2)
            .HasDefaultValue(0m)
            .HasColumnName("discount_pct");

        builder.Property(e => e.VatRate)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("vat_rate");

        builder.Property(e => e.LineTotal)
            .HasPrecision(19, 4)
            .HasColumnName("line_total");

        builder.Property(e => e.Position)
            .HasDefaultValue(0)
            .HasColumnName("position");

        builder.HasOne(e => e.PurchaseOrder)
            .WithMany(o => o.Lines)
            .HasForeignKey(e => e.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
