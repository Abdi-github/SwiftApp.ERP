namespace SwiftApp.ERP.Modules.Crm.Domain.Interfaces;

public interface ICrmModuleApi
{
    Task<long> GetActiveContactCountAsync(CancellationToken ct = default);
    Task<long> GetUpcomingInteractionCountAsync(CancellationToken ct = default);
}
