using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Accounting.Application.DTOs;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.Modules.Accounting.Domain.Enums;
using SwiftApp.ERP.Modules.Accounting.Domain.Events;
using SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Accounting.Application.Services;

public class JournalEntryService(
    IJournalEntryRepository entryRepository,
    IAccountRepository accountRepository,
    IPublisher publisher,
    ILogger<JournalEntryService> logger)
{
    public async Task<JournalEntryResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await entryRepository.GetByIdAsync(id, ct);
        return entry is null ? null : MapToResponse(entry);
    }

    public async Task<JournalEntryResponse?> GetByEntryNumberAsync(string entryNumber, CancellationToken ct = default)
    {
        var entry = await entryRepository.GetByEntryNumberAsync(entryNumber, ct);
        return entry is null ? null : MapToResponse(entry);
    }

    public async Task<PagedResult<JournalEntryResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await entryRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<JournalEntryResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<List<JournalEntryResponse>> GetUnpostedAsync(CancellationToken ct = default)
    {
        var entries = await entryRepository.GetUnpostedAsync(ct);
        return entries.Select(MapToResponse).ToList();
    }

    public async Task<JournalEntryResponse> CreateAsync(JournalEntryRequest request, CancellationToken ct = default)
    {
        // Validate all referenced accounts exist
        foreach (var line in request.Lines)
        {
            _ = await accountRepository.GetByIdAsync(line.AccountId, ct)
                ?? throw new EntityNotFoundException(nameof(Account), line.AccountId);
        }

        var seq = await entryRepository.GetNextSequenceNumberAsync(ct);
        var entryNumber = $"JE-{seq:D6}";

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            Description = request.Description,
            EntryDate = request.EntryDate,
            Reference = request.Reference,
            SourceDocumentType = request.SourceDocumentType,
            SourceDocumentId = request.SourceDocumentId,
        };

        var position = 0;
        foreach (var lineReq in request.Lines)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = lineReq.AccountId,
                Description = lineReq.Description,
                Debit = lineReq.Debit,
                Credit = lineReq.Credit,
                Position = lineReq.Position ?? position,
            });
            position++;
        }

        await entryRepository.AddAsync(entry, ct);
        // System.Diagnostics.Debug.WriteLine($"Journal entry persisted: number={entry.EntryNumber}, id={entry.Id}");

        logger.LogInformation("Journal entry {EntryNumber} created with {LineCount} lines", entry.EntryNumber, entry.Lines.Count);

        // Reload to get navigation properties
        var saved = await entryRepository.GetByIdAsync(entry.Id, ct);
        return MapToResponse(saved!);
    }

    public async Task<JournalEntryResponse> UpdateAsync(Guid id, JournalEntryRequest request, CancellationToken ct = default)
    {
        var entry = await entryRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(JournalEntry), id);

        if (entry.Posted)
            throw new BusinessRuleException("ENTRY_ALREADY_POSTED", "Cannot update a posted journal entry.");

        // Validate all referenced accounts exist
        foreach (var line in request.Lines)
        {
            _ = await accountRepository.GetByIdAsync(line.AccountId, ct)
                ?? throw new EntityNotFoundException(nameof(Account), line.AccountId);
        }

        entry.Description = request.Description;
        entry.EntryDate = request.EntryDate;
        entry.Reference = request.Reference;
        entry.SourceDocumentType = request.SourceDocumentType;
        entry.SourceDocumentId = request.SourceDocumentId;

        entry.Lines.Clear();
        var position = 0;
        foreach (var lineReq in request.Lines)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                JournalEntryId = id,
                AccountId = lineReq.AccountId,
                Description = lineReq.Description,
                Debit = lineReq.Debit,
                Credit = lineReq.Credit,
                Position = lineReq.Position ?? position,
            });
            position++;
        }

        await entryRepository.UpdateAsync(entry, ct);

        logger.LogInformation("Journal entry {EntryNumber} updated", entry.EntryNumber);

        var saved = await entryRepository.GetByIdAsync(entry.Id, ct);
        return MapToResponse(saved!);
    }

    public async Task<JournalEntryResponse> PostAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await entryRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(JournalEntry), id);

        if (entry.Posted)
            throw new BusinessRuleException("ENTRY_ALREADY_POSTED", "Journal entry is already posted.");

        if (entry.Lines.Count < 2)
            throw new BusinessRuleException("INSUFFICIENT_LINES", "Journal entry must have at least 2 lines.");

        var totalDebit = entry.Lines.Sum(l => l.Debit);
        var totalCredit = entry.Lines.Sum(l => l.Credit);
        // logger.LogDebug("Post check for {EntryNumber}: debit={TotalDebit}, credit={TotalCredit}", entry.EntryNumber, totalDebit, totalCredit);
        if (totalDebit != totalCredit)
            throw new BusinessRuleException("UNBALANCED_ENTRY", "Total debits must equal total credits.");

        // Update account balances
        foreach (var line in entry.Lines)
        {
            var account = await accountRepository.GetByIdAsync(line.AccountId, ct)
                ?? throw new EntityNotFoundException(nameof(Account), line.AccountId);

            account.Balance += GetBalanceEffect(account.AccountType, line.Debit, line.Credit);
            await accountRepository.UpdateAsync(account, ct);
        }

        entry.Posted = true;
        await entryRepository.UpdateAsync(entry, ct);

        logger.LogInformation("Journal entry {EntryNumber} posted, total {TotalAmount}", entry.EntryNumber, totalDebit);
        await publisher.Publish(new JournalEntryPostedEvent(entry.Id, entry.EntryNumber, totalDebit), ct);

        var saved = await entryRepository.GetByIdAsync(entry.Id, ct);
        return MapToResponse(saved!);
    }

    public async Task<JournalEntryResponse> ReverseAsync(Guid id, CancellationToken ct = default)
    {
        var original = await entryRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(JournalEntry), id);

        if (!original.Posted)
            throw new BusinessRuleException("ENTRY_NOT_POSTED", "Only posted entries can be reversed.");

        if (original.Reversed)
            throw new BusinessRuleException("ENTRY_ALREADY_REVERSED", "Journal entry is already reversed.");

        var seq = await entryRepository.GetNextSequenceNumberAsync(ct);
        var reversalNumber = $"JE-{seq:D6}";

        var reversal = new JournalEntry
        {
            EntryNumber = reversalNumber,
            Description = $"Reversal of {original.EntryNumber}",
            EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Reference = original.EntryNumber,
            SourceDocumentType = "REVERSAL",
            SourceDocumentId = original.Id,
        };

        foreach (var line in original.Lines)
        {
            reversal.Lines.Add(new JournalEntryLine
            {
                AccountId = line.AccountId,
                Description = $"Reversal: {line.Description}",
                Debit = line.Credit,   // Swap debit/credit
                Credit = line.Debit,
                Position = line.Position,
            });
        }

        await entryRepository.AddAsync(reversal, ct);

        // Update account balances (reverse the original effects)
        foreach (var line in reversal.Lines)
        {
            var account = await accountRepository.GetByIdAsync(line.AccountId, ct)
                ?? throw new EntityNotFoundException(nameof(Account), line.AccountId);

            account.Balance += GetBalanceEffect(account.AccountType, line.Debit, line.Credit);
            await accountRepository.UpdateAsync(account, ct);
        }

        reversal.Posted = true;
        await entryRepository.UpdateAsync(reversal, ct);

        original.Reversed = true;
        await entryRepository.UpdateAsync(original, ct);

        logger.LogInformation("Journal entry {OriginalNumber} reversed by {ReversalNumber}",
            original.EntryNumber, reversal.EntryNumber);
        await publisher.Publish(new JournalEntryReversedEvent(
            original.Id, reversal.Id, original.EntryNumber, reversal.EntryNumber), ct);

        var saved = await entryRepository.GetByIdAsync(reversal.Id, ct);
        return MapToResponse(saved!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await entryRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(JournalEntry), id);

        if (entry.Posted)
            throw new BusinessRuleException("ENTRY_POSTED", "Cannot delete a posted journal entry. Use reverse instead.");

        // Soft delete lines via cascade or manual
        foreach (var line in entry.Lines)
        {
            line.DeletedAt = DateTimeOffset.UtcNow;
        }

        entry.DeletedAt = DateTimeOffset.UtcNow;
        await entryRepository.UpdateAsync(entry, ct);

        logger.LogInformation("Journal entry {EntryNumber} soft-deleted", entry.EntryNumber);
    }

    /// <summary>
    /// Asset/Expense: debit increases, credit decreases.
    /// Liability/Equity/Revenue: credit increases, debit decreases.
    /// </summary>
    private static decimal GetBalanceEffect(AccountType accountType, decimal debit, decimal credit) =>
        accountType switch
        {
            AccountType.Asset or AccountType.Expense => debit - credit,
            AccountType.Liability or AccountType.Equity or AccountType.Revenue => credit - debit,
            _ => 0m,
        };

    private static JournalEntryResponse MapToResponse(JournalEntry entry) => new(
        entry.Id,
        entry.EntryNumber,
        entry.Description,
        entry.EntryDate,
        entry.Posted,
        entry.Reversed,
        entry.Reference,
        entry.SourceDocumentType,
        entry.SourceDocumentId,
        entry.Lines.Sum(l => l.Debit),
        entry.Lines.Sum(l => l.Credit),
        entry.Lines.OrderBy(l => l.Position).Select(MapLineToResponse).ToList(),
        entry.CreatedAt,
        entry.UpdatedAt);

    private static JournalEntryLineResponse MapLineToResponse(JournalEntryLine line) => new(
        line.Id,
        line.AccountId,
        line.Account?.AccountNumber ?? string.Empty,
        line.Account?.Name ?? string.Empty,
        line.Description,
        line.Debit,
        line.Credit,
        line.Position);
}
