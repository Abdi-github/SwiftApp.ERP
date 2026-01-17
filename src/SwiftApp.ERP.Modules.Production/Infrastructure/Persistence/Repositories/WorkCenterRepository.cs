using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Repositories;

public class WorkCenterRepository(AppDbContext db) : IWorkCenterRepository
{
    public async Task<WorkCenter?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<WorkCenter>()
            .Include(w => w.Translations)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<WorkCenter?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.Set<WorkCenter>()
            .Include(w => w.Translations)
            .FirstOrDefaultAsync(w => w.Code == code, ct);

    public async Task<PagedResult<WorkCenter>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<WorkCenter>()
            .Include(w => w.Translations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(w =>
                EF.Functions.ILike(w.Code, $"%{term}%") ||
                EF.Functions.ILike(w.Name, $"%{term}%"));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderBy(w => w.Code)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<WorkCenter>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<WorkCenter>> GetAllActiveAsync(CancellationToken ct = default)
        => await db.Set<WorkCenter>()
            .Include(w => w.Translations)
            .Where(w => w.Active)
            .OrderBy(w => w.Code)
            .ToListAsync(ct);

    public async Task AddAsync(WorkCenter workCenter, CancellationToken ct = default)
    {
        await db.Set<WorkCenter>().AddAsync(workCenter, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WorkCenter workCenter, CancellationToken ct = default)
    {
        db.Set<WorkCenter>().Update(workCenter);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var wc = await db.Set<WorkCenter>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(WorkCenter), id);
        wc.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
