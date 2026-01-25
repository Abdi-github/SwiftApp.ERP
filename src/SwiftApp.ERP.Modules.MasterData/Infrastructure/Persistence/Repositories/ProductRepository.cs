using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        => await db.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task<PagedResult<Product>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.Translations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Sku, $"%{term}%") ||
                EF.Functions.ILike(p.Name, $"%{term}%"));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"ProductRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderBy(p => p.Sku)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Product>(items, page, size, totalItems, totalPages);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await db.Set<Product>().AddAsync(product, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        db.Set<Product>().Update(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await db.Set<Product>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Product), id);
        product.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
