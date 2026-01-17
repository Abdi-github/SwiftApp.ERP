using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public class WarehouseRepository(AppDbContext db) : IWarehouseRepository
{
    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"WarehouseRepository.GetById include translations for warehouseId={id}");
        => await db.Set<Warehouse>()
            .Include(w => w.Translations)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.Set<Warehouse>()
            .Include(w => w.Translations)
            .FirstOrDefaultAsync(w => w.Code == code, ct);

    public async Task<PagedResult<Warehouse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Warehouse>()
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

        return new PagedResult<Warehouse>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllActiveAsync(CancellationToken ct = default)
        => await db.Set<Warehouse>()
            .Include(w => w.Translations)
            .Where(w => w.Active)
            .OrderBy(w => w.Code)
            .ToListAsync(ct);

    public async Task AddAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        await db.Set<Warehouse>().AddAsync(warehouse, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        db.Set<Warehouse>().Update(warehouse);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await db.Set<Warehouse>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Warehouse), id);
        // System.Diagnostics.Debug.WriteLine($"Warehouse soft delete request for id={id}");
        warehouse.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
