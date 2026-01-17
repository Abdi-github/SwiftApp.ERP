using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Configurations;

public class WorkCenterConfiguration : BaseEntityConfiguration<WorkCenter>
{
    protected override void ConfigureEntity(EntityTypeBuilder<WorkCenter> builder)
    {
        builder.ToTable("work_centers");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("code");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(e => e.CapacityPerDay)
            .HasPrecision(19, 4)
            .HasDefaultValue(1m)
            .HasColumnName("capacity_per_day");

        builder.Property(e => e.CostPerHour)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("cost_per_hour");

        builder.Property(e => e.Active)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.HasMany(e => e.Translations)
            .WithOne(t => t.WorkCenter)
            .HasForeignKey(t => t.WorkCenterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Code).IsUnique();
    }
}
