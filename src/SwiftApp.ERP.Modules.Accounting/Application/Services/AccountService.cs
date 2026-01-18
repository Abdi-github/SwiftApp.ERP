using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Accounting.Application.DTOs;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.Modules.Accounting.Domain.Enums;
using SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Accounting.Application.Services;

public class AccountService(
    IAccountRepository accountRepository,
    ILogger<AccountService> logger)
{
    public async Task<AccountResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await accountRepository.GetByIdAsync(id, ct);
        return account is null ? null : MapToResponse(account);
    }

    public async Task<AccountResponse?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
    {
        var account = await accountRepository.GetByAccountNumberAsync(accountNumber, ct);
        return account is null ? null : MapToResponse(account);
    }

    public async Task<PagedResult<AccountResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await accountRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<AccountResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<List<AccountResponse>> GetByTypeAsync(AccountType accountType, CancellationToken ct = default)
    {
        var accounts = await accountRepository.GetByTypeAsync(accountType, ct);
        return accounts.Select(MapToResponse).ToList();
    }

    public async Task<List<AccountResponse>> GetRootAccountsAsync(CancellationToken ct = default)
    {
        var accounts = await accountRepository.GetRootAccountsAsync(ct);
        return accounts.Select(MapToResponse).ToList();
    }

    public async Task<List<AccountResponse>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        var accounts = await accountRepository.GetChildrenAsync(parentId, ct);
        return accounts.Select(MapToResponse).ToList();
    }

    public async Task<AccountResponse> CreateAsync(AccountRequest request, CancellationToken ct = default)
    {
        var existing = await accountRepository.GetByAccountNumberAsync(request.AccountNumber, ct);
        if (existing is not null)
            throw new BusinessRuleException("ACCOUNT_NUMBER_EXISTS", $"Account number '{request.AccountNumber}' already exists.");

        if (request.ParentId.HasValue)
        {
            _ = await accountRepository.GetByIdAsync(request.ParentId.Value, ct)
                ?? throw new EntityNotFoundException(nameof(Account), request.ParentId.Value);
        }

        var account = new Account
        {
            AccountNumber = request.AccountNumber,
            Name = request.Name,
            Description = request.Description,
            AccountType = Enum.Parse<AccountType>(request.AccountType, ignoreCase: true),
            ParentId = request.ParentId,
            Active = request.Active ?? true,
        };

        await accountRepository.AddAsync(account, ct);

        logger.LogInformation("Account {AccountNumber} '{Name}' created", account.AccountNumber, account.Name);

        return MapToResponse(account);
    }

    public async Task<AccountResponse> UpdateAsync(Guid id, AccountRequest request, CancellationToken ct = default)
    {
        var account = await accountRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Account), id);

        var existing = await accountRepository.GetByAccountNumberAsync(request.AccountNumber, ct);
        if (existing is not null && existing.Id != id)
            throw new BusinessRuleException("ACCOUNT_NUMBER_EXISTS", $"Account number '{request.AccountNumber}' already exists.");

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                throw new BusinessRuleException("SELF_PARENT", "An account cannot be its own parent.");

            _ = await accountRepository.GetByIdAsync(request.ParentId.Value, ct)
                ?? throw new EntityNotFoundException(nameof(Account), request.ParentId.Value);
        }

        account.AccountNumber = request.AccountNumber;
        account.Name = request.Name;
        account.Description = request.Description;
        account.AccountType = Enum.Parse<AccountType>(request.AccountType, ignoreCase: true);
        account.ParentId = request.ParentId;
        account.Active = request.Active ?? account.Active;

        await accountRepository.UpdateAsync(account, ct);

        logger.LogInformation("Account {AccountNumber} updated", account.AccountNumber);

        return MapToResponse(account);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await accountRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Account), id);

        var children = await accountRepository.GetChildrenAsync(id, ct);
        if (children.Count > 0)
            throw new BusinessRuleException("ACCOUNT_HAS_CHILDREN", "Cannot delete account with child accounts.");

        if (account.Balance != 0)
            throw new BusinessRuleException("ACCOUNT_HAS_BALANCE", "Cannot delete account with non-zero balance.");

        await accountRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Account {AccountNumber} soft-deleted", account.AccountNumber);
    }

    private static AccountResponse MapToResponse(Account account) => new(
        account.Id,
        account.AccountNumber,
        account.Name,
        account.Description,
        account.AccountType.ToString(),
        account.ParentId,
        account.Parent?.Name,
        account.Active,
        account.Balance,
        account.CreatedAt,
        account.UpdatedAt);
}
