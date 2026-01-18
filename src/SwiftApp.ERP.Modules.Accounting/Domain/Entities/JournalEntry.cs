using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Accounting.Domain.Entities;

public class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EntryDate { get; set; }
    public bool Posted { get; set; }
    public bool Reversed { get; set; }
    public string? Reference { get; set; }
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public ICollection<JournalEntryLine> Lines { get; set; } = [];
}
