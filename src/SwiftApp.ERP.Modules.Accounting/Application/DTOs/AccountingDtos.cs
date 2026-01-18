namespace SwiftApp.ERP.Modules.Accounting.Application.DTOs;

public record AccountRequest(
    string AccountNumber,
    string Name,
    string? Description,
    string AccountType,
    Guid? ParentId,
    bool? Active);

public record AccountResponse(
    Guid Id,
    string AccountNumber,
    string Name,
    string? Description,
    string AccountType,
    Guid? ParentId,
    string? ParentName,
    bool Active,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record JournalEntryRequest(
    string? Description,
    DateOnly EntryDate,
    string? Reference,
    string? SourceDocumentType,
    Guid? SourceDocumentId,
    List<JournalEntryLineRequest> Lines);

public record JournalEntryResponse(
    Guid Id,
    string EntryNumber,
    string? Description,
    DateOnly EntryDate,
    bool Posted,
    bool Reversed,
    string? Reference,
    string? SourceDocumentType,
    Guid? SourceDocumentId,
    decimal TotalDebit,
    decimal TotalCredit,
    List<JournalEntryLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record JournalEntryLineRequest(
    Guid AccountId,
    string? Description,
    decimal Debit,
    decimal Credit,
    int? Position);

public record JournalEntryLineResponse(
    Guid Id,
    Guid AccountId,
    string AccountNumber,
    string AccountName,
    string? Description,
    decimal Debit,
    decimal Credit,
    int Position);
