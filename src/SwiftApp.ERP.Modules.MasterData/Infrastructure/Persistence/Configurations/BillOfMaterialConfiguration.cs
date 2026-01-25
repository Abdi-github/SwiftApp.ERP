using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Configurations;

public class BillOfMaterialConfiguration : BaseEntityConfiguration<BillOfMaterial>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BillOfMaterial> builder)
    {
        builder.ToTable("bill_of_materials");

        builder.Property(e => e.ProductId)
            .IsRequired()
            .HasColumnName("product_id");

        builder.Property(e => e.MaterialId)
            .IsRequired()
            .HasColumnName("material_id");

        builder.Property(e => e.Quantity)
            .HasPrecision(19, 4)
            .HasColumnName("quantity");

        builder.Property(e => e.UnitOfMeasureId)
            .HasColumnName("unit_of_measure_id");

        builder.Property(e => e.Position)
            .IsRequired()
            .HasColumnName("position");

        builder.Property(e => e.Notes)
            .HasMaxLength(500)
            .HasColumnName("notes");

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Material)
            .WithMany()
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(e => e.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.ProductId, e.MaterialId }).IsUnique();
    }
}
