using SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Accounting.Infrastructure;

public class AccountingModuleApiFacade(
    IAccountRepository accountRepository,
    IJournalEntryRepository journalEntryRepository) : IAccountingModuleApi
{
    public async Task<decimal> GetAccountBalanceAsync(string accountNumber, CancellationToken ct = default)
    {
        var account = await accountRepository.GetByAccountNumberAsync(accountNumber, ct);
        return account?.Balance ?? 0m;
    }

    public async Task<long> GetUnpostedEntryCountAsync(CancellationToken ct = default)
        => await journalEntryRepository.GetUnpostedCountAsync(ct);
}
