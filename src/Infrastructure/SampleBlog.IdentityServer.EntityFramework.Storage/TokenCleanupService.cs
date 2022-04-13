using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.EntityFramework.Storage.Options;

namespace SampleBlog.IdentityServer.EntityFramework.Storage;

public class TokenCleanupService
{
    private readonly OperationalStoreOptions options;
    private readonly IPersistedGrantDbContext dbContext;
    //private readonly IOperationalStoreNotification _operationalStoreNotification;
    //private readonly IServerSideSessionsMarker _sideSessionsMarker;
    private readonly ILogger<TokenCleanupService> logger;

    /// <summary>
    /// Constructor for TokenCleanupService.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="persistedGrantDbContext"></param>
    /// <param name="operationalStoreNotification"></param>
    /// <param name="logger"></param>
    /// <param name="serverSideSessionsMarker"></param>
    public TokenCleanupService(
        OperationalStoreOptions options,
        IPersistedGrantDbContext persistedGrantDbContext,
        ILogger<TokenCleanupService> logger
        //IServerSideSessionsMarker serverSideSessionsMarker = null,
        //IOperationalStoreNotification operationalStoreNotification = null
        )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));

        if (1 > this.options.TokenCleanupBatchSize)
        {
            throw new ArgumentException("Token cleanup batch size interval must be at least 1");
        }

        dbContext = persistedGrantDbContext ?? throw new ArgumentNullException(nameof(persistedGrantDbContext));

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        //_operationalStoreNotification = operationalStoreNotification;
        //_sideSessionsMarker = serverSideSessionsMarker;
    }

    /// <summary>
    /// Method to clear expired persisted grants.
    /// </summary>
    /// <returns></returns>
    public async Task RemoveExpiredGrantsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogTrace("Querying for expired grants to remove");

            await RemoveGrantsAsync(cancellationToken);
            await RemoveDeviceCodesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError("Exception removing expired grants: {exception}", ex.Message);
        }
    }

    /// <summary>
    /// Removes the stale persisted grants.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task RemoveGrantsAsync(CancellationToken cancellationToken = default)
    {
        await RemoveExpiredPersistedGrantsAsync(cancellationToken);
        if (options.RemoveConsumedTokens)
        {
            await RemoveConsumedPersistedGrantsAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Removes the expired persisted grants.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task RemoveExpiredPersistedGrantsAsync(CancellationToken cancellationToken = default)
    {
        var found = Int32.MaxValue;

        while (found >= options.TokenCleanupBatchSize)
        {
            var expiredGrants = await dbContext.PersistedGrants
                .Where(x => x.Expiration < DateTime.UtcNow)
                .OrderBy(x => x.Expiration)
                .Take(options.TokenCleanupBatchSize)
                .ToArrayAsync(cancellationToken);

            found = expiredGrants.Length;

            if (0 < found)
            {
                logger.LogInformation("Removing {grantCount} expired grants", found);

                dbContext.PersistedGrants.RemoveRange(expiredGrants);

                /*var list = await _persistedGrantDbContext.SaveChangesWithConcurrencyCheckAsync<Entities.PersistedGrant>(_logger, cancellationToken);

                expiredGrants = expiredGrants.Except(list).ToArray();

                if (_operationalStoreNotification != null)
                {
                    await _operationalStoreNotification.PersistedGrantsRemovedAsync(expiredGrants);
                }*/
            }
        }
    }

    /// <summary>
    /// Removes the consumed persisted grants.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task RemoveConsumedPersistedGrantsAsync(CancellationToken cancellationToken = default)
    {
        var found = Int32.MaxValue;

        while (found >= options.TokenCleanupBatchSize)
        {
            var expiredGrants = await dbContext.PersistedGrants
                .Where(x => x.ConsumedTime < DateTime.UtcNow)
                .OrderBy(x => x.ConsumedTime)
                .Take(options.TokenCleanupBatchSize)
                .ToArrayAsync(cancellationToken);

            found = expiredGrants.Length;

            if (found > 0)
            {
                logger.LogInformation("Removing {grantCount} consumed grants", found);

                dbContext.PersistedGrants.RemoveRange(expiredGrants);

                /*var list = await _persistedGrantDbContext.SaveChangesWithConcurrencyCheckAsync<Entities.PersistedGrant>(_logger, cancellationToken);
                expiredGrants = expiredGrants.Except(list).ToArray();

                if (_operationalStoreNotification != null)
                {
                    await _operationalStoreNotification.PersistedGrantsRemovedAsync(expiredGrants);
                }*/
            }
        }
    }


    /// <summary>
    /// Removes the stale device codes.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task RemoveDeviceCodesAsync(CancellationToken cancellationToken = default)
    {
        var found = Int32.MaxValue;

        while (found >= options.TokenCleanupBatchSize)
        {
            /*var expiredCodes = await _persistedGrantDbContext.DeviceFlowCodes
                .Where(x => x.Expiration < DateTime.UtcNow)
                .OrderBy(x => x.DeviceCode)
                .Take(_options.TokenCleanupBatchSize)
                .ToArrayAsync(cancellationToken);

            found = expiredCodes.Length;

            if (found > 0)
            {
                _logger.LogInformation("Removing {deviceCodeCount} device flow codes", found);

                _persistedGrantDbContext.DeviceFlowCodes.RemoveRange(expiredCodes);

                var list = await _persistedGrantDbContext.SaveChangesWithConcurrencyCheckAsync<Entities.DeviceFlowCodes>(_logger, cancellationToken);
                expiredCodes = expiredCodes.Except(list).ToArray();

                if (_operationalStoreNotification != null)
                {
                    await _operationalStoreNotification.DeviceCodesRemovedAsync(expiredCodes);
                }
            }*/
            
            break;  // remove after uncomment
        }
    }
}