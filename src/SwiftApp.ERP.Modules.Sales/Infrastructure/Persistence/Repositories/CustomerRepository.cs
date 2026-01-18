using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Sales.Infrastructure.Persistence.Repositories;

public class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Customer?> GetByCustomerNumberAsync(string customerNumber, CancellationToken ct = default)
        => await db.Set<Customer>().FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber, ct);

    public async Task<PagedResult<Customer>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Customer>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                EF.Functions.ILike(c.CustomerNumber, $"%{term}%") ||
                (c.CompanyName != null && EF.Functions.ILike(c.CompanyName, $"%{term}%")) ||
                (c.FirstName != null && EF.Functions.ILike(c.FirstName, $"%{term}%")) ||
                (c.LastName != null && EF.Functions.ILike(c.LastName, $"%{term}%")) ||
                (c.Email != null && EF.Functions.ILike(c.Email, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"Sales CustomerRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderBy(c => c.CustomerNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Customer>(items, page, size, totalItems, totalPages);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await db.Set<Customer>().AddAsync(customer, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        db.Set<Customer>().Update(customer);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await db.Set<Customer>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Customer), id);
        customer.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default)
    {
        var prefix = $"C-{year}-";
        var maxNumber = await db.Set<Customer>()
            .IgnoreQueryFilters()
            .Where(c => c.CustomerNumber.StartsWith(prefix))
            .Select(c => c.CustomerNumber)
            .MaxAsync(cancellationToken: ct);

        if (maxNumber is null)
            return 0;

        var seqPart = maxNumber[prefix.Length..];
        return int.TryParse(seqPart, out var seq) ? seq : 0;
    }
}
