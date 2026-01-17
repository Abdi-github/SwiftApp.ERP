namespace SwiftApp.ERP.Modules.Inventory.Application.DTOs;

// ── Warehouse ───────────────────────────────────────────────────────────

public record WarehouseRequest(
    string Code,
    string Name,
    string? Description,
    string? Address,
    bool? Active,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record WarehouseResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Address,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

// ── StockLevel ──────────────────────────────────────────────────────────

public record StockLevelResponse(
    Guid Id,
    Guid ItemId,
    string ItemType,
    string ItemName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable);

// ── StockMovement ───────────────────────────────────────────────────────

public record StockMovementRequest(
    string MovementType,
    Guid ItemId,
    string ItemType,
    Guid? SourceWarehouseId,
    Guid? TargetWarehouseId,
    decimal Quantity,
    string? Reason);

public record StockMovementResponse(
    Guid Id,
    string ReferenceNumber,
    string MovementType,
    Guid ItemId,
    string ItemType,
    Guid? SourceWarehouseId,
    string? SourceWarehouseCode,
    Guid? TargetWarehouseId,
    string? TargetWarehouseCode,
    decimal Quantity,
    DateTimeOffset MovementDate,
    string? Reason,
    string? SourceDocumentType,
    Guid? SourceDocumentId,
    DateTimeOffset CreatedAt);
