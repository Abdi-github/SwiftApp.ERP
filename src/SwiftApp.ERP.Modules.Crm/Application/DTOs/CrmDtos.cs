namespace SwiftApp.ERP.Modules.Crm.Application.DTOs;

// ── Contact ─────────────────────────────────────────────────────────────

public record ContactRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string? Street,
    string? City,
    string? PostalCode,
    string? Canton,
    string? Country,
    Guid? CustomerId,
    string? Notes,
    bool? Active);

public record ContactResponse(
    Guid Id,
    string? FirstName,
    string? LastName,
    string DisplayName,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string? Street,
    string? City,
    string? PostalCode,
    string? Canton,
    string? Country,
    Guid? CustomerId,
    string? Notes,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── Interaction ─────────────────────────────────────────────────────────

public record InteractionRequest(
    Guid ContactId,
    string InteractionType,
    string Subject,
    string? Description,
    DateTimeOffset? InteractionDate,
    DateTimeOffset? FollowUpDate,
    Guid? AssignedTo,
    bool? Completed);

public record InteractionResponse(
    Guid Id,
    Guid ContactId,
    string ContactName,
    string InteractionType,
    string Subject,
    string? Description,
    DateTimeOffset InteractionDate,
    DateTimeOffset? FollowUpDate,
    Guid? AssignedTo,
    bool Completed,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
