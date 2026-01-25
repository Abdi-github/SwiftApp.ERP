using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;

public interface IQualityCheckRepository
{
    Task<QualityCheck?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<QualityCheck?> GetByCheckNumberAsync(string checkNumber, CancellationToken ct = default);
    Task<PagedResult<QualityCheck>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<QualityCheck>> GetByInspectionPlanAsync(Guid inspectionPlanId, CancellationToken ct = default);
    Task<string> GetNextCheckNumberAsync(CancellationToken ct = default);
    Task AddAsync(QualityCheck check, CancellationToken ct = default);
    Task UpdateAsync(QualityCheck check, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
