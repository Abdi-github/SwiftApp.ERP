using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.QualityControl.Infrastructure.Persistence.Repositories;

public class NonConformanceReportRepository(AppDbContext db) : INonConformanceReportRepository
{
    public async Task<NonConformanceReport?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<NonConformanceReport>()
            .Include(e => e.QualityCheck)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<NonConformanceReport?> GetByNcrNumberAsync(string ncrNumber, CancellationToken ct = default)
        => await db.Set<NonConformanceReport>()
            .Include(e => e.QualityCheck)
            .FirstOrDefaultAsync(e => e.NcrNumber == ncrNumber, ct);

    public async Task<PagedResult<NonConformanceReport>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<NonConformanceReport>()
            .Include(e => e.QualityCheck)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.NcrNumber, $"%{term}%") ||
                (e.Description != null && EF.Functions.ILike(e.Description, $"%{term}%")) ||
                (e.ResponsiblePerson != null && EF.Functions.ILike(e.ResponsiblePerson, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"NCR repository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<NonConformanceReport>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<NonConformanceReport>> GetByStatusAsync(NcrStatus status, CancellationToken ct = default)
        => await db.Set<NonConformanceReport>()
            .Include(e => e.QualityCheck)
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<NonConformanceReport>> GetByQualityCheckAsync(Guid qualityCheckId, CancellationToken ct = default)
        => await db.Set<NonConformanceReport>()
            .Include(e => e.QualityCheck)
            .Where(e => e.QualityCheckId == qualityCheckId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public async Task<string> GetNextNcrNumberAsync(CancellationToken ct = default)
    {
        var lastNumber = await db.Set<NonConformanceReport>()
            .IgnoreQueryFilters()
            .OrderByDescending(e => e.NcrNumber)
            .Select(e => e.NcrNumber)
            .FirstOrDefaultAsync(ct);

        if (lastNumber is not null && lastNumber.StartsWith("NCR-") && int.TryParse(lastNumber[4..], out var seq))
            return $"NCR-{(seq + 1):D5}";

        return "NCR-00001";
    }

    public async Task AddAsync(NonConformanceReport ncr, CancellationToken ct = default)
    {
        await db.Set<NonConformanceReport>().AddAsync(ncr, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NonConformanceReport ncr, CancellationToken ct = default)
    {
        db.Set<NonConformanceReport>().Update(ncr);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ncr = await db.Set<NonConformanceReport>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(NonConformanceReport), id);
        ncr.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
