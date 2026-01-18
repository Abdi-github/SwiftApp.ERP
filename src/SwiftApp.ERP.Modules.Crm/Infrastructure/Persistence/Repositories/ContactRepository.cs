using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.Modules.Crm.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Crm.Infrastructure.Persistence.Repositories;

public class ContactRepository(AppDbContext db) : IContactRepository
{
    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Contact>()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<PagedResult<Contact>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Contact>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                (e.FirstName != null && EF.Functions.ILike(e.FirstName, $"%{term}%")) ||
                (e.LastName != null && EF.Functions.ILike(e.LastName, $"%{term}%")) ||
                (e.Email != null && EF.Functions.ILike(e.Email, $"%{term}%")) ||
                (e.Company != null && EF.Functions.ILike(e.Company, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Contact>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<Contact>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => await db.Set<Contact>()
            .Where(e => e.CustomerId == customerId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(ct);

    public async Task AddAsync(Contact contact, CancellationToken ct = default)
    {
        await db.Set<Contact>().AddAsync(contact, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Contact contact, CancellationToken ct = default)
    {
        db.Set<Contact>().Update(contact);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var contact = await db.Set<Contact>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Contact), id);
        contact.DeletedAt = DateTimeOffset.UtcNow;
        contact.Active = false;
        await db.SaveChangesAsync(ct);
    }
}
