using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.QualityControl.Infrastructure.Persistence.Repositories;

public class InspectionPlanRepository(AppDbContext db) : IInspectionPlanRepository
{
    public async Task<InspectionPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<InspectionPlan>()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<InspectionPlan?> GetByPlanNumberAsync(string planNumber, CancellationToken ct = default)
        => await db.Set<InspectionPlan>()
            .FirstOrDefaultAsync(e => e.PlanNumber == planNumber, ct);

    public async Task<PagedResult<InspectionPlan>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<InspectionPlan>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.PlanNumber, $"%{term}%") ||
                (e.Name != null && EF.Functions.ILike(e.Name, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"InspectionPlanRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderBy(e => e.PlanNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<InspectionPlan>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<InspectionPlan>> GetActiveAsync(CancellationToken ct = default)
        => await db.Set<InspectionPlan>()
            .Where(e => e.Active)
            .OrderBy(e => e.PlanNumber)
            .ToListAsync(ct);

    public async Task<string> GetNextPlanNumberAsync(CancellationToken ct = default)
    {
        var lastNumber = await db.Set<InspectionPlan>()
            .IgnoreQueryFilters()
            .OrderByDescending(e => e.PlanNumber)
            .Select(e => e.PlanNumber)
            .FirstOrDefaultAsync(ct);

        if (lastNumber is not null && lastNumber.StartsWith("IP-") && int.TryParse(lastNumber[3..], out var seq))
            return $"IP-{(seq + 1):D5}";

        return "IP-00001";
    }

    public async Task AddAsync(InspectionPlan plan, CancellationToken ct = default)
    {
        await db.Set<InspectionPlan>().AddAsync(plan, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(InspectionPlan plan, CancellationToken ct = default)
    {
        db.Set<InspectionPlan>().Update(plan);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await db.Set<InspectionPlan>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(InspectionPlan), id);
        plan.DeletedAt = DateTimeOffset.UtcNow;
        plan.Active = false;
        await db.SaveChangesAsync(ct);
    }
}
