using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Application.DTOs;

// ── Product ─────────────────────────────────────────────────────────────

public record ProductRequest(
    string Sku,
    string Name,
    string? Description,
    Guid? CategoryId,
    decimal UnitPrice,
    decimal ListPrice,
    VatRate VatRate,
    bool? Active,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    decimal UnitPrice,
    decimal ListPrice,
    string VatRate,
    bool Active,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── Material ────────────────────────────────────────────────────────────

public record MaterialRequest(
    string Sku,
    string Name,
    string? Description,
    Guid? CategoryId,
    Guid? UnitOfMeasureId,
    decimal UnitPrice,
    VatRate VatRate,
    decimal MinimumStock,
    bool? Active,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record MaterialResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    Guid? UnitOfMeasureId,
    string? UnitOfMeasureCode,
    decimal UnitPrice,
    string VatRate,
    decimal MinimumStock,
    bool Active,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── Category ────────────────────────────────────────────────────────────

public record CategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── UnitOfMeasure ───────────────────────────────────────────────────────

public record UnitOfMeasureRequest(
    string Code,
    string Name,
    string? Description,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations);

public record UnitOfMeasureResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Dictionary<string, string>? NameTranslations,
    Dictionary<string, string>? DescriptionTranslations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── BillOfMaterial ──────────────────────────────────────────────────────

public record BillOfMaterialRequest(
    Guid MaterialId,
    decimal Quantity,
    Guid? UnitOfMeasureId,
    int Position,
    string? Notes);

public record BillOfMaterialResponse(
    Guid Id,
    Guid ProductId,
    Guid MaterialId,
    string MaterialSku,
    string MaterialName,
    decimal Quantity,
    Guid? UnitOfMeasureId,
    string? UnitOfMeasureCode,
    int Position,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
