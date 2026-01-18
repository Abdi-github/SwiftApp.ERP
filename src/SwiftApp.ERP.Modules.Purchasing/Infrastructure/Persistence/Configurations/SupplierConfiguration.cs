using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : BaseEntityConfiguration<Supplier>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.Property(e => e.SupplierNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("supplier_number");

        builder.Property(e => e.CompanyName)
            .HasMaxLength(255)
            .HasColumnName("company_name");

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

        builder.Property(e => e.VatNumber)
            .HasMaxLength(30)
            .HasColumnName("vat_number");

        builder.Property(e => e.PaymentTerms)
            .HasDefaultValue(30)
            .HasColumnName("payment_terms");

        builder.Property(e => e.ContactPerson)
            .HasMaxLength(255)
            .HasColumnName("contact_person");

        builder.Property(e => e.Website)
            .HasMaxLength(500)
            .HasColumnName("website");

        builder.Property(e => e.Notes)
            .HasColumnType("text")
            .HasColumnName("notes");

        builder.Property(e => e.Active)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.Ignore(e => e.DisplayName);

        builder.HasIndex(e => e.SupplierNumber).IsUnique();
    }
}
