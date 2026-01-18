using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Crm.Infrastructure.Persistence.Configurations;

public class InteractionConfiguration : BaseEntityConfiguration<Interaction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Interaction> builder)
    {
        builder.ToTable("interactions");

        builder.Property(e => e.ContactId)
            .IsRequired()
            .HasColumnName("contact_id");

        builder.Property(e => e.InteractionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("interaction_type");

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("subject");

        builder.Property(e => e.Description)
            .HasColumnName("description");

        builder.Property(e => e.InteractionDate)
            .IsRequired()
            .HasColumnName("interaction_date");

        builder.Property(e => e.FollowUpDate)
            .HasColumnName("follow_up_date");

        builder.Property(e => e.AssignedTo)
            .HasColumnName("assigned_to");

        builder.Property(e => e.Completed)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("completed");

        builder.HasOne(e => e.Contact)
            .WithMany(c => c.Interactions)
            .HasForeignKey(e => e.ContactId);

        builder.HasIndex(e => e.ContactId);
        builder.HasIndex(e => e.FollowUpDate);
    }
}
