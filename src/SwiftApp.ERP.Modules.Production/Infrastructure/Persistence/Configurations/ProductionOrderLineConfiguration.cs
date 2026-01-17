using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Configurations;

public class ProductionOrderLineConfiguration : BaseEntityConfiguration<ProductionOrderLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ProductionOrderLine> builder)
    {
        builder.ToTable("production_order_lines");

        builder.Property(e => e.ProductionOrderId)
            .IsRequired()
            .HasColumnName("production_order_id");

        builder.Property(e => e.MaterialId)
            .IsRequired()
            .HasColumnName("material_id");

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(e => e.RequiredQuantity)
            .HasPrecision(19, 4)
            .HasColumnName("required_quantity");

        builder.Property(e => e.IssuedQuantity)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("issued_quantity");

        builder.Property(e => e.Position)
            .HasDefaultValue(0)
            .HasColumnName("position");

        builder.HasOne(e => e.ProductionOrder)
            .WithMany(o => o.Lines)
            .HasForeignKey(e => e.ProductionOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProductionOrderId);
    }
}
