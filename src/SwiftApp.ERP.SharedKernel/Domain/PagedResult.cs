namespace SwiftApp.ERP.SharedKernel.Domain;

/// <summary>
/// Generic paged result wrapper for list endpoints.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalItems,
    int TotalPages
);
