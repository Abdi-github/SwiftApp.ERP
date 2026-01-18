using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Crm.Domain.Entities;

public class Contact : BaseEntity
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Company { get; set; }

    public string? Position { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Canton { get; set; }

    public string Country { get; set; } = "CH";

    public Guid? CustomerId { get; set; }

    public string? Notes { get; set; }

    public bool Active { get; set; } = true;

    public string DisplayName => $"{FirstName} {LastName}".Trim();

    public ICollection<Interaction> Interactions { get; set; } = [];
}
