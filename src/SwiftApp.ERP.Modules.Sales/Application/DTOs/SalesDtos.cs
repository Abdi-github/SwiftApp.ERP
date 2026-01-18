namespace SwiftApp.ERP.Modules.Sales.Application.DTOs;

// ── Customer ────────────────────────────────────────────────────────────

public record CustomerRequest(
    string? CustomerNumber,
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
    decimal? CreditLimit,
    string? Notes,
    bool? Active);

public record CustomerResponse(
    Guid Id,
    string CustomerNumber,
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
    decimal CreditLimit,
    string? Notes,
    bool Active,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── SalesOrder ──────────────────────────────────────────────────────────

public record SalesOrderRequest(
    Guid CustomerId,
    DateOnly? OrderDate,
    DateOnly? DeliveryDate,
    string? Notes,
    string? ShippingStreet,
    string? ShippingCity,
    string? ShippingPostalCode,
    string? ShippingCanton,
    string? ShippingCountry,
    List<SalesOrderLineRequest>? Lines);

public record SalesOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string Status,
    DateOnly OrderDate,
    DateOnly? DeliveryDate,
    decimal Subtotal,
    decimal VatAmount,
    decimal TotalAmount,
    string Currency,
    string? Notes,
    string? ShippingStreet,
    string? ShippingCity,
    string? ShippingPostalCode,
    string? ShippingCanton,
    string? ShippingCountry,
    List<SalesOrderLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ── SalesOrderLine ──────────────────────────────────────────────────────

public record SalesOrderLineRequest(
    Guid ProductId,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal? DiscountPct,
    string VatRate,
    int? Position);

public record SalesOrderLineResponse(
    Guid Id,
    Guid ProductId,
    string? Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPct,
    string VatRate,
    decimal LineTotal,
    decimal VatAmount,
    int Position);
