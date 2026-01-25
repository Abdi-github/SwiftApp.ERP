using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.QualityControl.Infrastructure;

public class QualityControlModuleApiFacade(AppDbContext db) : IQualityControlModuleApi
{
    public async Task<long> GetOpenNcrCountAsync(CancellationToken ct = default)
        => await db.Set<NonConformanceReport>()
            .Where(n => n.Status != NcrStatus.Closed)
            .LongCountAsync(ct);

    public async Task<decimal> GetPassRateAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var totalChecks = await db.Set<QualityCheck>()
            .Where(c => c.CheckDate >= from && c.CheckDate <= to)
            .LongCountAsync(ct);

        if (totalChecks == 0)
            return 0m;

        var passedChecks = await db.Set<QualityCheck>()
            .Where(c => c.CheckDate >= from && c.CheckDate <= to && c.Result == QualityCheckResult.Pass)
            .LongCountAsync(ct);

        return Math.Round((decimal)passedChecks / totalChecks * 100, 2);
    }
}
