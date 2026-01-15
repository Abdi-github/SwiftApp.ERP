using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.SharedKernel.Interfaces;

namespace SwiftApp.ERP.SharedKernel.Services;

/// <summary>
/// Redis-backed distributed cache via IDistributedCache.
/// Falls back gracefully on Redis unavailability.
/// </summary>
public class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        try
        {
            var data = await cache.GetStringAsync(key, ct);
            if (data is null) return null;
            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis GET failed for key {Key}, returning default", key);
            return null;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        try
        {
            var data = await cache.GetStringAsync(key, ct);
            if (data is not null)
            {
                var cached = JsonSerializer.Deserialize<T>(data, JsonOptions);
                if (cached is not null) return cached;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis GET failed for key {Key}, calling factory", key);
        }

        var value = await factory();

        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };
            await cache.SetStringAsync(key, json, options, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis SET failed for key {Key}", key);
        }

        return value;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        try
        {
            var data = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };
            await cache.SetStringAsync(key, data, options, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis SET failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis REMOVE failed for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            logger.LogDebug("RemoveByPrefix called for {Prefix} — requires direct Redis connection for SCAN", prefix);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis REMOVE_BY_PREFIX failed for prefix {Prefix}", prefix);
        }
    }
}
