using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.Modules.Hr.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Hr.Infrastructure.Persistence.Repositories;

public class DepartmentRepository(AppDbContext db) : IDepartmentRepository
{
    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"DepartmentRepository.GetById include graph for departmentId={id}");
        => await db.Set<Department>()
            .Include(d => d.Translations)
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<Department?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.Set<Department>()
            .Include(d => d.Translations)
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Code == code, ct);

    public async Task<PagedResult<Department>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Department>()
            .Include(d => d.Translations)
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d =>
                EF.Functions.ILike(d.Code, $"%{term}%") ||
                EF.Functions.ILike(d.Name, $"%{term}%"));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"DepartmentRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderBy(d => d.Code)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Department>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<Department>> GetAllActiveAsync(CancellationToken ct = default)
        => await db.Set<Department>()
            .Include(d => d.Translations)
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .Where(d => d.Active)
            .OrderBy(d => d.Code)
            .ToListAsync(ct);

    public async Task AddAsync(Department department, CancellationToken ct = default)
    {
        await db.Set<Department>().AddAsync(department, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Department department, CancellationToken ct = default)
    {
        db.Set<Department>().Update(department);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var department = await db.Set<Department>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Department), id);
        department.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
