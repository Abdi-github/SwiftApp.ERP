using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;

public interface IJournalEntryRepository
{
    Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken ct = default);
    Task<PagedResult<JournalEntry>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<List<JournalEntry>> GetUnpostedAsync(CancellationToken ct = default);
    Task AddAsync(JournalEntry entry, CancellationToken ct = default);
    Task UpdateAsync(JournalEntry entry, CancellationToken ct = default);
    Task<int> GetNextSequenceNumberAsync(CancellationToken ct = default);
    Task<long> GetUnpostedCountAsync(CancellationToken ct = default);
}
