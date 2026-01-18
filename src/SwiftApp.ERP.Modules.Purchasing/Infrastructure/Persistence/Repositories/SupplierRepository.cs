using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Purchasing.Infrastructure.Persistence.Repositories;

public class SupplierRepository(AppDbContext db) : ISupplierRepository
{
    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Supplier>().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Supplier?> GetBySupplierNumberAsync(string supplierNumber, CancellationToken ct = default)
        => await db.Set<Supplier>().FirstOrDefaultAsync(s => s.SupplierNumber == supplierNumber, ct);

    public async Task<PagedResult<Supplier>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Supplier>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s =>
                EF.Functions.ILike(s.SupplierNumber, $"%{term}%") ||
                (s.CompanyName != null && EF.Functions.ILike(s.CompanyName, $"%{term}%")) ||
                (s.FirstName != null && EF.Functions.ILike(s.FirstName, $"%{term}%")) ||
                (s.LastName != null && EF.Functions.ILike(s.LastName, $"%{term}%")) ||
                (s.Email != null && EF.Functions.ILike(s.Email, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"SupplierRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderBy(s => s.SupplierNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Supplier>(items, page, size, totalItems, totalPages);
    }

    public async Task AddAsync(Supplier supplier, CancellationToken ct = default)
    {
        await db.Set<Supplier>().AddAsync(supplier, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        db.Set<Supplier>().Update(supplier);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await db.Set<Supplier>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Supplier), id);
        supplier.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default)
    {
        var prefix = $"S-{year}-";
        var maxNumber = await db.Set<Supplier>()
            .IgnoreQueryFilters()
            .Where(s => s.SupplierNumber.StartsWith(prefix))
            .Select(s => s.SupplierNumber)
            .MaxAsync(cancellationToken: ct);

        if (maxNumber is null)
            return 0;

        var seqPart = maxNumber[prefix.Length..];
        return int.TryParse(seqPart, out var seq) ? seq : 0;
    }
}
