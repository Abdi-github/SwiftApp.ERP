using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.QualityControl.Infrastructure.Persistence.Configurations;

public class NonConformanceReportConfiguration : BaseEntityConfiguration<NonConformanceReport>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NonConformanceReport> builder)
    {
        builder.ToTable("non_conformance_reports");

        builder.Property(e => e.NcrNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("ncr_number");

        builder.Property(e => e.QualityCheckId)
            .HasColumnName("quality_check_id");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(e => e.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("severity");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(NcrStatus.Open)
            .HasColumnName("status");

        builder.Property(e => e.RootCause)
            .HasColumnType("text")
            .HasColumnName("root_cause");

        builder.Property(e => e.CorrectiveAction)
            .HasColumnType("text")
            .HasColumnName("corrective_action");

        builder.Property(e => e.ResponsiblePerson)
            .HasMaxLength(255)
            .HasColumnName("responsible_person");

        builder.Property(e => e.DueDate)
            .HasColumnName("due_date");

        builder.Property(e => e.ClosedDate)
            .HasColumnName("closed_date");

        builder.HasIndex(e => e.NcrNumber).IsUnique();
    }
}
