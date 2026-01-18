using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Sales.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : BaseEntityConfiguration<SalesOrder>
{
    protected override void ConfigureEntity(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");

        builder.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("order_number");

        builder.Property(e => e.CustomerId)
            .IsRequired()
            .HasColumnName("customer_id");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("status");

        builder.Property(e => e.OrderDate)
            .IsRequired()
            .HasColumnName("order_date");

        builder.Property(e => e.DeliveryDate)
            .HasColumnName("delivery_date");

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

        builder.Property(e => e.ShippingStreet)
            .HasMaxLength(500)
            .HasColumnName("shipping_street");

        builder.Property(e => e.ShippingCity)
            .HasMaxLength(100)
            .HasColumnName("shipping_city");

        builder.Property(e => e.ShippingPostalCode)
            .HasMaxLength(20)
            .HasColumnName("shipping_postal_code");

        builder.Property(e => e.ShippingCanton)
            .HasMaxLength(50)
            .HasColumnName("shipping_canton");

        builder.Property(e => e.ShippingCountry)
            .HasMaxLength(3)
            .HasColumnName("shipping_country");

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Lines)
            .WithOne(l => l.SalesOrder)
            .HasForeignKey(l => l.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.OrderNumber).IsUnique();
    }
}
