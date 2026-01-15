using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.Modules.Hr.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Hr.Infrastructure.Persistence.Repositories;

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Employee>()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default)
        => await db.Set<Employee>()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber, ct);

    public async Task<PagedResult<Employee>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<Employee>()
            .Include(e => e.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                EF.Functions.ILike(e.EmployeeNumber, $"%{term}%") ||
                EF.Functions.ILike(e.FirstName, $"%{term}%") ||
                EF.Functions.ILike(e.LastName, $"%{term}%") ||
                (e.Email != null && EF.Functions.ILike(e.Email, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
        // System.Diagnostics.Debug.WriteLine($"Hr EmployeeRepository paging page={page}, size={size}, total={totalItems}");

        var items = await query
            .OrderBy(e => e.EmployeeNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Employee>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        => await db.Set<Employee>()
            .Include(e => e.Department)
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> GetActiveAsync(CancellationToken ct = default)
        => await db.Set<Employee>()
            .Include(e => e.Department)
            .Where(e => e.Active)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(ct);

    public async Task<string> GetNextEmployeeNumberAsync(CancellationToken ct = default)
    {
        var lastNumber = await db.Set<Employee>()
            .IgnoreQueryFilters()
            .OrderByDescending(e => e.EmployeeNumber)
            .Select(e => e.EmployeeNumber)
            .FirstOrDefaultAsync(ct);

        if (lastNumber is not null && lastNumber.StartsWith("EMP-") && int.TryParse(lastNumber[4..], out var seq))
            return $"EMP-{(seq + 1):D5}";

        return "EMP-00001";
    }

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
    {
        await db.Set<Employee>().AddAsync(employee, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        db.Set<Employee>().Update(employee);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await db.Set<Employee>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Employee), id);
        employee.DeletedAt = DateTimeOffset.UtcNow;
        employee.Active = false;
        await db.SaveChangesAsync(ct);
    }
}
