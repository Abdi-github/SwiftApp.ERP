using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Accounting.Infrastructure.Persistence.Repositories;

public class JournalEntryRepository(AppDbContext db) : IJournalEntryRepository
{
    public async Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"JournalEntryRepository.GetById include lines/accounts for entryId={id}");
        => await db.Set<JournalEntry>()
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<JournalEntry?> GetByEntryNumberAsync(string entryNumber, CancellationToken ct = default)
        => await db.Set<JournalEntry>()
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(e => e.EntryNumber == entryNumber, ct);

    public async Task<PagedResult<JournalEntry>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<JournalEntry>()
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.EntryNumber, $"%{term}%") ||
                (e.Description != null && EF.Functions.ILike(e.Description, $"%{term}%")) ||
                (e.Reference != null && EF.Functions.ILike(e.Reference, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.EntryNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<JournalEntry>(items, page, size, totalItems, totalPages);
    }

    public async Task<List<JournalEntry>> GetUnpostedAsync(CancellationToken ct = default)
        => await db.Set<JournalEntry>()
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .Where(e => !e.Posted)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync(ct);

    public async Task AddAsync(JournalEntry entry, CancellationToken ct = default)
    {
        await db.Set<JournalEntry>().AddAsync(entry, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(JournalEntry entry, CancellationToken ct = default)
    {
        db.Set<JournalEntry>().Update(entry);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetNextSequenceNumberAsync(CancellationToken ct = default)
    {
        var maxNumber = await db.Set<JournalEntry>()
            .IgnoreQueryFilters()
            .Select(e => e.EntryNumber)
            .MaxAsync(ct);

        if (maxNumber is null)
            return 1;

        // Format is JE-NNNNNN, extract the numeric part
        var seqPart = maxNumber["JE-".Length..];
        return int.TryParse(seqPart, out var seq) ? seq + 1 : 1;
    }

    public async Task<long> GetUnpostedCountAsync(CancellationToken ct = default)
        => await db.Set<JournalEntry>()
            .Where(e => !e.Posted)
            .LongCountAsync(ct);
}
