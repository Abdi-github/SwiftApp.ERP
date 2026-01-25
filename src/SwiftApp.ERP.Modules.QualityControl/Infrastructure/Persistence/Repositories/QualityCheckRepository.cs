using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.QualityControl.Infrastructure.Persistence.Repositories;

public class QualityCheckRepository(AppDbContext db) : IQualityCheckRepository
{
    public async Task<QualityCheck?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<QualityCheck>()
            .Include(e => e.InspectionPlan)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<QualityCheck?> GetByCheckNumberAsync(string checkNumber, CancellationToken ct = default)
        => await db.Set<QualityCheck>()
            .Include(e => e.InspectionPlan)
            .FirstOrDefaultAsync(e => e.CheckNumber == checkNumber, ct);

    public async Task<PagedResult<QualityCheck>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<QualityCheck>()
            .Include(e => e.InspectionPlan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.CheckNumber, $"%{term}%") ||
                (e.InspectorName != null && EF.Functions.ILike(e.InspectorName, $"%{term}%")) ||
                (e.BatchNumber != null && EF.Functions.ILike(e.BatchNumber, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"QualityCheckRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderByDescending(e => e.CheckDate)
            .ThenBy(e => e.CheckNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<QualityCheck>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<QualityCheck>> GetByInspectionPlanAsync(Guid inspectionPlanId, CancellationToken ct = default)
        => await db.Set<QualityCheck>()
            .Include(e => e.InspectionPlan)
            .Where(e => e.InspectionPlanId == inspectionPlanId)
            .OrderByDescending(e => e.CheckDate)
            .ToListAsync(ct);

    public async Task<string> GetNextCheckNumberAsync(CancellationToken ct = default)
    {
        var lastNumber = await db.Set<QualityCheck>()
            .IgnoreQueryFilters()
            .OrderByDescending(e => e.CheckNumber)
            .Select(e => e.CheckNumber)
            .FirstOrDefaultAsync(ct);

        if (lastNumber is not null && lastNumber.StartsWith("QC-") && int.TryParse(lastNumber[3..], out var seq))
            return $"QC-{(seq + 1):D5}";

        return "QC-00001";
    }

    public async Task AddAsync(QualityCheck check, CancellationToken ct = default)
    {
        await db.Set<QualityCheck>().AddAsync(check, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(QualityCheck check, CancellationToken ct = default)
    {
        db.Set<QualityCheck>().Update(check);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var check = await db.Set<QualityCheck>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(QualityCheck), id);
        check.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
