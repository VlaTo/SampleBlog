using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Implementation of IBackchannelAuthenticationThrottlingService that uses the IDistributedCache.
/// </summary>
public class DistributedBackChannelAuthenticationThrottlingService : IBackChannelAuthenticationThrottlingService
{
    private readonly IDistributedCache cache;
    private readonly IClientStore clientStore;
    private readonly ISystemClock clock;
    private readonly IdentityServerOptions options;

    private const string KeyPrefix = "backchannel_";

    /// <summary>
    /// Ctor
    /// </summary>
    public DistributedBackChannelAuthenticationThrottlingService(
        IDistributedCache cache,
        IClientStore clientStore,
        ISystemClock clock,
        IdentityServerOptions options)
    {
        this.cache = cache;
        this.clientStore = clientStore;
        this.clock = clock;
        this.options = options;
    }

    /// <inheritdoc/>
    public async Task<bool> ShouldSlowDown(string requestId, BackChannelAuthenticationRequest details)
    {
        using var activity = Tracing.ServiceActivitySource.StartActivity("DistributedBackchannelAuthenticationThrottlingService.ShouldSlowDown");

        if (null == requestId)
        {
            throw new ArgumentNullException(nameof(requestId));
        }

        var key = KeyPrefix + requestId;
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = clock.UtcNow + details.Lifetime
        };

        var lastSeenAsString = await cache.GetStringAsync(key);

        // record new
        if (null == lastSeenAsString)
        {
            await cache.SetStringAsync(key, clock.UtcNow.ToString("O"), cacheOptions);
            return false;
        }

        // check interval
        if (DateTime.TryParse(lastSeenAsString, out var lastSeen))
        {
            lastSeen = lastSeen.ToUniversalTime();

            var client = await clientStore.FindEnabledClientByIdAsync(details.ClientId);
            var interval = client?.PollingInterval ?? options.Ciba.DefaultPollingInterval;

            if ((lastSeen + interval) > clock.UtcNow.UtcDateTime)
            {
                await cache.SetStringAsync(key, clock.UtcNow.ToString("O"), cacheOptions);
                return true;
            }
        }

        // store current and continue
        await cache.SetStringAsync(key, clock.UtcNow.ToString("O"), cacheOptions);

        return false;
    }
}