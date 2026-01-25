using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.QualityControl.Infrastructure.Persistence.Configurations;

public class QualityCheckConfiguration : BaseEntityConfiguration<QualityCheck>
{
    protected override void ConfigureEntity(EntityTypeBuilder<QualityCheck> builder)
    {
        builder.ToTable("quality_checks");

        builder.Property(e => e.CheckNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("check_number");

        builder.Property(e => e.InspectionPlanId)
            .HasColumnName("inspection_plan_id");

        builder.Property(e => e.InspectorName)
            .HasMaxLength(255)
            .HasColumnName("inspector_name");

        builder.Property(e => e.CheckDate)
            .IsRequired()
            .HasColumnName("check_date");

        builder.Property(e => e.Result)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("result");

        builder.Property(e => e.ItemId)
            .HasColumnName("item_id");

        builder.Property(e => e.BatchNumber)
            .HasMaxLength(50)
            .HasColumnName("batch_number");

        builder.Property(e => e.SampleSize)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("sample_size");

        builder.Property(e => e.DefectCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("defect_count");

        builder.Property(e => e.Notes)
            .HasColumnType("text")
            .HasColumnName("notes");

        builder.HasMany(e => e.NonConformanceReports)
            .WithOne(n => n.QualityCheck)
            .HasForeignKey(n => n.QualityCheckId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.CheckNumber).IsUnique();
    }
}
