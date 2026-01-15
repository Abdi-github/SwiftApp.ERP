namespace SwiftApp.ERP.Modules.Hr.Application.DTOs;

// ── Department ──────────────────────────────────────────────────────────

public record DepartmentRequest(
    string Code,
    string Name,
    string? Description,
    Guid? ManagerId,
    bool? Active,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record DepartmentResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ManagerId,
    string? ManagerName,
    bool Active,
    int EmployeeCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

// ── Employee ────────────────────────────────────────────────────────────

public record EmployeeRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    Guid DepartmentId,
    string? Position,
    DateOnly HireDate,
    DateOnly? TerminationDate,
    decimal? Salary,
    string? Street,
    string? City,
    string? PostalCode,
    string? Canton,
    string? Country,
    bool? Active);

public record EmployeeResponse(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string DisplayName,
    string? Email,
    string? Phone,
    Guid DepartmentId,
    string DepartmentName,
    string? Position,
    DateOnly HireDate,
    DateOnly? TerminationDate,
    decimal Salary,
    string? Street,
    string? City,
    string? PostalCode,
    string? Canton,
    string? Country,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
