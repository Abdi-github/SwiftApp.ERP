using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.Modules.Accounting.Domain.Enums;
using SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Accounting.Infrastructure.Persistence.Repositories;

public class AccountRepository(AppDbContext db) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"AccountRepository.GetById include Parent for accountId={id}");
        => await db.Set<Account>()
            .Include(a => a.Parent)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
        => await db.Set<Account>()
            .Include(a => a.Parent)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);

    public async Task<PagedResult<Account>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Account>()
            .Include(a => a.Parent)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                EF.Functions.ILike(a.AccountNumber, $"%{term}%") ||
                EF.Functions.ILike(a.Name, $"%{term}%"));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderBy(a => a.AccountNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Account>(items, page, size, totalItems, totalPages);
    }

    public async Task<List<Account>> GetByTypeAsync(AccountType accountType, CancellationToken ct = default)
        => await db.Set<Account>()
            .Include(a => a.Parent)
            .Where(a => a.AccountType == accountType)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(ct);

    public async Task<List<Account>> GetRootAccountsAsync(CancellationToken ct = default)
        => await db.Set<Account>()
            .Where(a => a.ParentId == null)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(ct);

    public async Task<List<Account>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
        => await db.Set<Account>()
            .Where(a => a.ParentId == parentId)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(ct);

    public async Task AddAsync(Account account, CancellationToken ct = default)
    {
        await db.Set<Account>().AddAsync(account, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Account account, CancellationToken ct = default)
    {
        db.Set<Account>().Update(account);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await db.Set<Account>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Account), id);
        // System.Diagnostics.Debug.WriteLine($"Account soft delete request for id={id}");
        account.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
