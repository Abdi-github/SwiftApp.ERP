namespace SwiftApp.ERP.SharedKernel.Interfaces;

/// <summary>
/// Abstraction for distributed caching (Redis).
/// Used by services to cache frequently-accessed data.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    /// <summary>
    /// Gets a value from cache or loads it via the factory if not found.
    /// Works correctly for both value types and reference types.
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default);
}
