using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Hr.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : BaseEntityConfiguration<Employee>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.Property(e => e.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("employee_number");

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("last_name");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(e => e.Phone)
            .HasMaxLength(50)
            .HasColumnName("phone");

        builder.Property(e => e.DepartmentId)
            .IsRequired()
            .HasColumnName("department_id");

        builder.Property(e => e.Position)
            .HasMaxLength(255)
            .HasColumnName("position");

        builder.Property(e => e.HireDate)
            .IsRequired()
            .HasColumnName("hire_date");

        builder.Property(e => e.TerminationDate)
            .HasColumnName("termination_date");

        builder.Property(e => e.Salary)
            .HasPrecision(19, 4)
            .HasDefaultValue(0m)
            .HasColumnName("salary");

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

        builder.Property(e => e.Active)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("active");

        builder.Ignore(e => e.DisplayName);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EmployeeNumber).IsUnique();
    }
}
