using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Accounting.Domain.Entities;

public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int Position { get; set; }
}
