using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Hr.Infrastructure.Persistence.Configurations;

public class DepartmentTranslationConfiguration : BaseTranslationConfiguration<DepartmentTranslation>
{
    protected override void ConfigureTranslation(EntityTypeBuilder<DepartmentTranslation> builder)
    {
        builder.ToTable("department_translations");

        builder.Property(e => e.DepartmentId)
            .IsRequired()
            .HasColumnName("department_id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Translations)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.DepartmentId, e.Locale }).IsUnique();
    }
}
