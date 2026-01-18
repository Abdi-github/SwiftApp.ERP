namespace SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;

public interface IAccountingModuleApi
{
    Task<decimal> GetAccountBalanceAsync(string accountNumber, CancellationToken ct = default);
    Task<long> GetUnpostedEntryCountAsync(CancellationToken ct = default);
}
