using Microsoft.Extensions.Caching.Distributed;
using SampleBlog.IdentityServer.Core;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Default implementation of the replay cache using IDistributedCache
/// </summary>
public class DefaultReplayCache : IReplayCache
{
    private const string Prefix = nameof(DefaultReplayCache) + "-";

    private readonly IDistributedCache cache;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="cache"></param>
    public DefaultReplayCache(IDistributedCache cache)
    {
        this.cache = cache;
    }

    /// <inheritdoc />
    public async Task AddAsync(string purpose, string handle, DateTimeOffset expiration)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultReplayCache.Add");

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        };

        await cache.SetAsync(Prefix + purpose + handle, Array.Empty<byte>(), options);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string purpose, string handle)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultReplayCache.Exists");

        var value = await cache.GetAsync(Prefix + purpose + handle);

        return null != value;
    }
}