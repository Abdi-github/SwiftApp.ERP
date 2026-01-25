using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;

public interface INonConformanceReportRepository
{
    Task<NonConformanceReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<NonConformanceReport?> GetByNcrNumberAsync(string ncrNumber, CancellationToken ct = default);
    Task<PagedResult<NonConformanceReport>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<NonConformanceReport>> GetByStatusAsync(NcrStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<NonConformanceReport>> GetByQualityCheckAsync(Guid qualityCheckId, CancellationToken ct = default);
    Task<string> GetNextNcrNumberAsync(CancellationToken ct = default);
    Task AddAsync(NonConformanceReport ncr, CancellationToken ct = default);
    Task UpdateAsync(NonConformanceReport ncr, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
