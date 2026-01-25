using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Configurations;

public class UnitOfMeasureConfiguration : BaseEntityConfiguration<UnitOfMeasure>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("units_of_measure");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("code");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("description");

        builder.HasMany(e => e.Translations)
            .WithOne(t => t.UnitOfMeasure)
            .HasForeignKey(t => t.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Code).IsUnique();
    }
}
