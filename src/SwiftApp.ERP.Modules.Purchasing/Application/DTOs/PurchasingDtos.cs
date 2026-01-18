namespace SwiftApp.ERP.Modules.Purchasing.Application.DTOs;

// ── Supplier ────────────────────────────────────────────────────────────

public record SupplierRequest(
    string? SupplierNumber,
    string? CompanyName,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Street,
    string? City,
    string? PostalCode,
    string? Canton,
    string? Country,
    string? VatNumber,
    int? PaymentTerms,
    string? ContactPerson,
    string? Website,
    string? Notes,
    bool? Active);

public record SupplierResponse(
    Guid Id,
    string SupplierNumber,
    string? CompanyName,
    string? FirstName,
    string? LastName,
    string DisplayName,
    string? Email,
    string? Phone,
    string? Street,
    string? City,
    string? PostalCode,
    string? Canton,
    string? Country,
    string? VatNumber,
    int PaymentTerms,
    string? ContactPerson,
    string? Website,
    string? Notes,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── PurchaseOrder ───────────────────────────────────────────────────────

public record PurchaseOrderRequest(
    Guid SupplierId,
    DateOnly? OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string? Notes,
    List<PurchaseOrderLineRequest>? Lines);

public record PurchaseOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    string Status,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    DateOnly? ActualDeliveryDate,
    decimal Subtotal,
    decimal VatAmount,
    decimal TotalAmount,
    string Currency,
    string? Notes,
    List<PurchaseOrderLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── PurchaseOrderLine ───────────────────────────────────────────────────

public record PurchaseOrderLineRequest(
    Guid MaterialId,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal? DiscountPct,
    string VatRate,
    int? Position);

public record PurchaseOrderLineResponse(
    Guid Id,
    Guid MaterialId,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPct,
    string VatRate,
    decimal LineTotal,
    decimal VatAmount,
    int Position);
