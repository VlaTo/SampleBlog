using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// The default device flow throttling service using IDistributedCache.
/// </summary>
/// <seealso cref="IDeviceFlowThrottlingService" />
public class DistributedDeviceFlowThrottlingService : IDeviceFlowThrottlingService
{
    private readonly IDistributedCache cache;
    private readonly IClientStore clientStore;
    private readonly ISystemClock clock;
    private readonly IdentityServerOptions options;

    private const string KeyPrefix = "devicecode_";

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedDeviceFlowThrottlingService"/> class.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <param name="clientStore"></param>
    /// <param name="clock">The clock.</param>
    /// <param name="options">The options.</param>
    public DistributedDeviceFlowThrottlingService(
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

    /// <summary>
    /// Decides if the requesting client and device code needs to slow down.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <param name="details">The device code details.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">deviceCode</exception>
    public async Task<bool> ShouldSlowDown(string deviceCode, DeviceCode details)
    {
        using var activity = Tracing.ServiceActivitySource.StartActivity("DistributedDeviceFlowThrottlingService.ShouldSlowDown");

        //if (deviceCode == null) throw new ArgumentNullException(nameof(deviceCode));

        var key = KeyPrefix + deviceCode;
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = clock.UtcNow.AddSeconds(details.Lifetime)
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
            var interval = client?.PollingInterval ?? options.DeviceFlow.Interval;

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