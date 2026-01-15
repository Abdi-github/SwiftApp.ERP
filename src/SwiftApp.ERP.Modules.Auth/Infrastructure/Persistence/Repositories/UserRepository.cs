using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.Modules.Auth.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"GetByIdAsync for userId={id} with Roles+Permissions include graph");
        => await db.Set<User>()
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await db.Set<User>()
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await db.Set<User>()
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<PagedResult<User>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<User>().Include(u => u.Roles).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(u =>
                EF.Functions.ILike(u.Username, $"%{term}%") ||
                EF.Functions.ILike(u.Email, $"%{term}%") ||
                EF.Functions.ILike(u.FirstName, $"%{term}%") ||
                EF.Functions.ILike(u.LastName, $"%{term}%"));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"User page request: page={page}, size={size}, total={totalItems}, totalPages={totalPages}");

        var items = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<User>(items, page, size, totalItems, totalPages);
    }

    public async Task<List<User>> GetAllEnabledAsync(CancellationToken ct = default)
        => await db.Set<User>()
            .Include(u => u.Roles)
            .Where(u => u.Enabled)
            .OrderBy(u => u.Username)
            .ToListAsync(ct);

    public async Task<List<User>> GetAllByRoleAsync(string roleName, CancellationToken ct = default)
        => await db.Set<User>()
            .Include(u => u.Roles)
            .Where(u => u.Roles.Any(r => r.Name == roleName))
            .OrderBy(u => u.Username)
            .ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await db.Set<User>().AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Set<User>().Update(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Set<User>().FindAsync([id], ct)
            ?? throw new SwiftApp.ERP.SharedKernel.Exceptions.EntityNotFoundException(nameof(User), id);
        user.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
