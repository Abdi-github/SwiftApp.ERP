using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.Modules.Hr.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Hr.Infrastructure;

public class HrModuleApiFacade(AppDbContext db) : IHrModuleApi
{
    public async Task<long> GetActiveEmployeeCountAsync(CancellationToken ct = default)
        => await db.Set<Employee>()
            .Where(e => e.Active)
            .LongCountAsync(ct);

    public async Task<long> GetDepartmentCountAsync(CancellationToken ct = default)
        => await db.Set<Department>()
            .Where(d => d.Active)
            .LongCountAsync(ct);
}
