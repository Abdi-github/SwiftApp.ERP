namespace SwiftApp.ERP.Modules.Sales.Domain.Interfaces;

public interface ISalesModuleApi
{
    Task<decimal> GetMonthlyRevenueAsync(int year, int month, CancellationToken ct = default);
    Task<long> GetOpenOrderCountAsync(CancellationToken ct = default);
    Task<decimal> GetOrderTotalAsync(Guid orderId, CancellationToken ct = default);
}
