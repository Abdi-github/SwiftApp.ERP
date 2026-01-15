using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Hr.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }

    public string? Position { get; set; }

    public DateOnly HireDate { get; set; }

    public DateOnly? TerminationDate { get; set; }

    public decimal Salary { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Canton { get; set; }

    public string? Country { get; set; } = "CH";

    public bool Active { get; set; } = true;

    public string DisplayName => $"{FirstName} {LastName}";
}
