using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Configurations;

public class WorkCenterTranslationConfiguration : BaseTranslationConfiguration<WorkCenterTranslation>
{
    protected override void ConfigureTranslation(EntityTypeBuilder<WorkCenterTranslation> builder)
    {
        builder.ToTable("work_center_translations");

        builder.Property(e => e.WorkCenterId)
            .IsRequired()
            .HasColumnName("work_center_id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.HasOne(e => e.WorkCenter)
            .WithMany(w => w.Translations)
            .HasForeignKey(e => e.WorkCenterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.WorkCenterId, e.Locale }).IsUnique();
    }
}
