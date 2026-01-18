using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : BaseEntityConfiguration<PurchaseOrder>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");

        builder.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("order_number");

        builder.Property(e => e.SupplierId)
            .IsRequired()
            .HasColumnName("supplier_id");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("status");

        builder.Property(e => e.OrderDate)
            .IsRequired()
            .HasColumnName("order_date");

        builder.Property(e => e.ExpectedDeliveryDate)
            .HasColumnName("expected_delivery_date");

        builder.Property(e => e.ActualDeliveryDate)
            .HasColumnName("actual_delivery_date");

        builder.Property(e => e.Subtotal)
            .HasPrecision(19, 4)
            .HasColumnName("subtotal");

        builder.Property(e => e.VatAmount)
            .HasPrecision(19, 4)
            .HasColumnName("vat_amount");

        builder.Property(e => e.TotalAmount)
            .HasPrecision(19, 4)
            .HasColumnName("total_amount");

        builder.Property(e => e.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("CHF")
            .HasColumnName("currency");

        builder.Property(e => e.Notes)
            .HasColumnType("text")
            .HasColumnName("notes");

        builder.HasOne(e => e.Supplier)
            .WithMany()
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Lines)
            .WithOne(l => l.PurchaseOrder)
            .HasForeignKey(l => l.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.OrderNumber).IsUnique();
    }
}
