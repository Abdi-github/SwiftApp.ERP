using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Configurations;

public class ProductionOrderConfiguration : BaseEntityConfiguration<ProductionOrder>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("production_orders");

        builder.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("order_number");

        builder.Property(e => e.ProductId)
            .IsRequired()
            .HasColumnName("product_id");

        builder.Property(e => e.WorkCenterId)
            .HasColumnName("work_center_id");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(ProductionOrderStatus.Draft)
            .HasColumnName("status");

        builder.Property(e => e.PlannedQuantity)
            .HasPrecision(19, 4)
            .HasColumnName("planned_quantity");

        builder.Property(e => e.CompletedQuantity)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("completed_quantity");

        builder.Property(e => e.ScrapQuantity)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("scrap_quantity");

        builder.Property(e => e.PlannedStartDate)
            .IsRequired()
            .HasColumnName("planned_start_date");

        builder.Property(e => e.PlannedEndDate)
            .IsRequired()
            .HasColumnName("planned_end_date");

        builder.Property(e => e.ActualStartDate)
            .HasColumnName("actual_start_date");

        builder.Property(e => e.ActualEndDate)
            .HasColumnName("actual_end_date");

        builder.Property(e => e.EstimatedCost)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("estimated_cost");

        builder.Property(e => e.ActualCost)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("actual_cost");

        builder.Property(e => e.Notes)
            .HasColumnType("text")
            .HasColumnName("notes");

        builder.HasOne(e => e.WorkCenter)
            .WithMany()
            .HasForeignKey(e => e.WorkCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Lines)
            .WithOne(l => l.ProductionOrder)
            .HasForeignKey(l => l.ProductionOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.OrderNumber).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ProductId);
    }
}
