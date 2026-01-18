using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Purchasing.Domain.Entities;

public class Supplier : BaseEntity
{
    public string SupplierNumber { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Canton { get; set; }

    public string? Country { get; set; } = "CH";

    public string? VatNumber { get; set; }

    public int PaymentTerms { get; set; } = 30;

    public string? ContactPerson { get; set; }

    public string? Website { get; set; }

    public string? Notes { get; set; }

    public bool Active { get; set; } = true;

    public string DisplayName => CompanyName ?? $"{FirstName} {LastName}".Trim();
}
