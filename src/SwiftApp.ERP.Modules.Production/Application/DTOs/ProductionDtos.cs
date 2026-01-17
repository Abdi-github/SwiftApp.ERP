namespace SwiftApp.ERP.Modules.Production.Application.DTOs;

public record WorkCenterRequest(
    string Code,
    string Name,
    string? Description,
    decimal? CapacityPerDay,
    decimal? CostPerHour,
    bool? Active,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record WorkCenterResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    decimal CapacityPerDay,
    decimal CostPerHour,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record ProductionOrderRequest(
    Guid ProductId,
    Guid? WorkCenterId,
    decimal PlannedQuantity,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    string? Notes,
    List<ProductionOrderLineRequest>? Lines);

public record ProductionOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    Guid? WorkCenterId,
    string? WorkCenterName,
    string Status,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    decimal ScrapQuantity,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    DateOnly? ActualStartDate,
    DateOnly? ActualEndDate,
    decimal EstimatedCost,
    decimal ActualCost,
    string? Notes,
    List<ProductionOrderLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record ProductionOrderLineRequest(
    Guid MaterialId,
    string? Description,
    decimal RequiredQuantity,
    int? Position);

public record ProductionOrderLineResponse(
    Guid Id,
    Guid MaterialId,
    string? Description,
    decimal RequiredQuantity,
    decimal IssuedQuantity,
    int Position);

public record CancelProductionOrderRequest(string Reason);

public record CompleteProductionOrderRequest(decimal CompletedQuantity, decimal? ScrapQuantity);
