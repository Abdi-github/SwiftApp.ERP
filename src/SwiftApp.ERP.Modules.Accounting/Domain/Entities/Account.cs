using SwiftApp.ERP.Modules.Accounting.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Accounting.Domain.Entities;

public class Account : BaseEntity
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccountType AccountType { get; set; }
    public Guid? ParentId { get; set; }
    public Account? Parent { get; set; }
    public bool Active { get; set; } = true;
    public decimal Balance { get; set; }
    public ICollection<Account> Children { get; set; } = [];
}
