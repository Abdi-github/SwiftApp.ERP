namespace SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;

public interface IPurchasingModuleApi
{
    Task<long> GetOpenPurchaseOrderCountAsync(CancellationToken ct = default);
    Task<decimal> GetMonthlySpendAsync(int year, int month, CancellationToken ct = default);
    Task<decimal> GetOrderTotalAsync(Guid orderId, CancellationToken ct = default);
}
