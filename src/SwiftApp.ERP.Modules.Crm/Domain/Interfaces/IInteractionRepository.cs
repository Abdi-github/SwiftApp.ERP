using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Crm.Domain.Interfaces;

public interface IInteractionRepository
{
    Task<Interaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Interaction>> GetPagedAsync(int page, int size, CancellationToken ct = default);
    Task<IReadOnlyList<Interaction>> GetByContactAsync(Guid contactId, CancellationToken ct = default);
    Task<IReadOnlyList<Interaction>> GetUpcomingAsync(DateTimeOffset from, CancellationToken ct = default);
    Task AddAsync(Interaction interaction, CancellationToken ct = default);
    Task UpdateAsync(Interaction interaction, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
