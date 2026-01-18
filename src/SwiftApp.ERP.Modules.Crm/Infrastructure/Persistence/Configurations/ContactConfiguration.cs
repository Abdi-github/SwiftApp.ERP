using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Crm.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : BaseEntityConfiguration<Contact>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .HasColumnName("last_name");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(e => e.Phone)
            .HasMaxLength(50)
            .HasColumnName("phone");

        builder.Property(e => e.Company)
            .HasMaxLength(255)
            .HasColumnName("company");

        builder.Property(e => e.Position)
            .HasMaxLength(255)
            .HasColumnName("position");

        builder.Property(e => e.Street)
            .HasMaxLength(500)
            .HasColumnName("street");

        builder.Property(e => e.City)
            .HasMaxLength(100)
            .HasColumnName("city");

        builder.Property(e => e.PostalCode)
            .HasMaxLength(20)
            .HasColumnName("postal_code");

        builder.Property(e => e.Canton)
            .HasMaxLength(50)
            .HasColumnName("canton");

        builder.Property(e => e.Country)
            .HasMaxLength(3)
            .HasDefaultValue("CH")
            .HasColumnName("country");

        builder.Property(e => e.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(e => e.Notes)
            .HasColumnName("notes");

        builder.Property(e => e.Active)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.Ignore(e => e.DisplayName);

        builder.HasMany(e => e.Interactions)
            .WithOne(i => i.Contact)
            .HasForeignKey(i => i.ContactId);

        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.CustomerId);
    }
}
