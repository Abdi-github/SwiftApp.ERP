namespace SwiftApp.ERP.Modules.QualityControl.Application.DTOs;

// ── InspectionPlan ──────────────────────────────────────────────────────

public record InspectionPlanRequest(
    string? Name,
    string? Description,
    Guid? ProductId,
    Guid? MaterialId,
    string? Criteria,
    string? Frequency,
    bool? Active);

public record InspectionPlanResponse(
    Guid Id,
    string PlanNumber,
    string? Name,
    string? Description,
    Guid? ProductId,
    Guid? MaterialId,
    string? Criteria,
    string? Frequency,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── QualityCheck ────────────────────────────────────────────────────────

public record QualityCheckRequest(
    Guid? InspectionPlanId,
    string? InspectorName,
    DateOnly CheckDate,
    string Result,
    Guid? ItemId,
    string? BatchNumber,
    int? SampleSize,
    int? DefectCount,
    string? Notes);

public record QualityCheckResponse(
    Guid Id,
    string CheckNumber,
    Guid? InspectionPlanId,
    string? PlanNumber,
    string? InspectorName,
    DateOnly CheckDate,
    string Result,
    Guid? ItemId,
    string? BatchNumber,
    int SampleSize,
    int DefectCount,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── NonConformanceReport ────────────────────────────────────────────────

public record NcrRequest(
    Guid? QualityCheckId,
    string? Description,
    string Severity,
    string? RootCause,
    string? CorrectiveAction,
    string? ResponsiblePerson,
    DateOnly? DueDate);

public record NcrResponse(
    Guid Id,
    string NcrNumber,
    Guid? QualityCheckId,
    string? CheckNumber,
    string? Description,
    string Severity,
    string Status,
    string? RootCause,
    string? CorrectiveAction,
    string? ResponsiblePerson,
    DateOnly? DueDate,
    DateOnly? ClosedDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
