using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Repositories;

public class MaterialRepository(AppDbContext db) : IMaterialRepository
{
    public async Task<Material?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Material>()
            .Include(m => m.Category)
            .Include(m => m.UnitOfMeasure)
            .Include(m => m.Translations)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Material?> GetBySkuAsync(string sku, CancellationToken ct = default)
        => await db.Set<Material>()
            .Include(m => m.Category)
            .Include(m => m.UnitOfMeasure)
            .Include(m => m.Translations)
            .FirstOrDefaultAsync(m => m.Sku == sku, ct);

    public async Task<PagedResult<Material>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Material>()
            .Include(m => m.Category)
            .Include(m => m.UnitOfMeasure)
            .Include(m => m.Translations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                EF.Functions.ILike(m.Sku, $"%{term}%") ||
                EF.Functions.ILike(m.Name, $"%{term}%"));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"MaterialRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderBy(m => m.Sku)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Material>(items, page, size, totalItems, totalPages);
    }

    public async Task AddAsync(Material material, CancellationToken ct = default)
    {
        await db.Set<Material>().AddAsync(material, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Material material, CancellationToken ct = default)
    {
        db.Set<Material>().Update(material);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var material = await db.Set<Material>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Material), id);
        material.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
