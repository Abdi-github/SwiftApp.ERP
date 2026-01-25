using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.QualityControl.Infrastructure.Persistence.Configurations;

public class InspectionPlanConfiguration : BaseEntityConfiguration<InspectionPlan>
{
    protected override void ConfigureEntity(EntityTypeBuilder<InspectionPlan> builder)
    {
        builder.ToTable("inspection_plans");

        builder.Property(e => e.PlanNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("plan_number");

        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.MaterialId)
            .HasColumnName("material_id");

        builder.Property(e => e.Criteria)
            .HasColumnType("text")
            .HasColumnName("criteria");

        builder.Property(e => e.Frequency)
            .HasMaxLength(100)
            .HasColumnName("frequency");

        builder.Property(e => e.Active)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.HasMany(e => e.QualityChecks)
            .WithOne(c => c.InspectionPlan)
            .HasForeignKey(c => c.InspectionPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.PlanNumber).IsUnique();
    }
}
