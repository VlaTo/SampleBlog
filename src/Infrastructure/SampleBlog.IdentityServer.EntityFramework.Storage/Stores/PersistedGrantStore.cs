using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;
using SampleBlog.IdentityServer.Storage;
using SampleBlog.IdentityServer.Storage.Extensions;
using SampleBlog.IdentityServer.Storage.Services;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Stores;

/// <summary>
/// Implementation of IPersistedGrantStore thats uses EF.
/// </summary>
/// <seealso cref="IPersistedGrantStore" />
public class PersistedGrantStore : IPersistedGrantStore
{
    /// <summary>
    /// The DbContext.
    /// </summary>
    protected readonly IPersistedGrantDbContext Context;

    /// <summary>
    /// The CancellationToken service.
    /// </summary>
    protected readonly ICancellationTokenProvider CancellationTokenProvider;

    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersistedGrantStore"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationTokenProvider"></param>
    public PersistedGrantStore(
        IPersistedGrantDbContext context,
        ICancellationTokenProvider cancellationTokenProvider,
        ILogger<PersistedGrantStore> logger)
    {
        Context = context;
        CancellationTokenProvider = cancellationTokenProvider;
        Logger = logger;
    }

    /// <inheritdoc/>
    public virtual async Task StoreAsync(IdentityServer.Storage.Models.PersistedGrant grant)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("PersistedGrantStore.StoreAsync");

        var existing = await Context.PersistedGrants
            .Where(x => x.Key == grant.Key)
            //.ToArrayAsync(CancellationTokenProvider.CancellationToken)
            .SingleOrDefaultAsync(x => x.Key == grant.Key, CancellationTokenProvider.CancellationToken);

        if (null == existing)
        {
            Logger.LogDebug("{persistedGrantKey} not found in database", grant.Key);
            
            var persistedGrant = grant.ToEntity();

            Context.PersistedGrants.Add(persistedGrant);
        }
        else
        {
            Logger.LogDebug("{persistedGrantKey} found in database", grant.Key);

            grant.UpdateEntity(existing);
        }

        try
        {
            await Context.SaveChangesAsync(CancellationTokenProvider.CancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", grant.Key, ex.Message);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IdentityServer.Storage.Models.PersistedGrant?> GetAsync(string key)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("PersistedGrantStore.GetAsync");

        var persistedGrant = await Context.PersistedGrants
            .Where(x => x.Key == key)
            //.ToArrayAsync(CancellationTokenProvider.CancellationToken)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, CancellationTokenProvider.CancellationToken);

        if (null != persistedGrant)
        {
            var model = persistedGrant.ToModel();

            Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return model;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IdentityServer.Storage.Models.PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("PersistedGrantStore.GetAllAsync");

        filter.Validate();

        var persistedGrants = await Filter(Context.PersistedGrants.AsQueryable(), filter)
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        persistedGrants = Filter(persistedGrants.AsQueryable(), filter).ToArray();

        var model = persistedGrants.Select(x => x.ToModel());

        Logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Length, filter);

        return model;
    }

    /// <inheritdoc/>
    public virtual async Task RemoveAsync(string key)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("PersistedGrantStore.RemoveAsync");

        var persistedGrant = (await Context.PersistedGrants.Where(x => x.Key == key)
                .ToArrayAsync(CancellationTokenProvider.CancellationToken))
            .SingleOrDefault(x => x.Key == key);
        if (persistedGrant != null)
        {
            Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

            Context.PersistedGrants.Remove(persistedGrant);

            try
            {
                await Context.SaveChangesAsync(CancellationTokenProvider.CancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
            }
        }
        else
        {
            Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAllAsync(PersistedGrantFilter filter)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("PersistedGrantStore.RemoveAllAsync");

        filter.Validate();

        var persistedGrants = await Filter(Context.PersistedGrants.AsQueryable(), filter)
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);
        persistedGrants = Filter(persistedGrants.AsQueryable(), filter).ToArray();

        Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", persistedGrants.Length, filter);

        Context.PersistedGrants.RemoveRange(persistedGrants);

        try
        {
            await Context.SaveChangesAsync(CancellationTokenProvider.CancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {@filter}: {error}", persistedGrants.Length, filter, ex.Message);
        }
    }


    private static IQueryable<PersistedGrant> Filter(IQueryable<PersistedGrant> query, PersistedGrantFilter filter)
    {
        if (null != filter.ClientIds)
        {
            var ids = filter.ClientIds.ToList();
            if (!String.IsNullOrWhiteSpace(filter.ClientId))
            {
                ids.Add(filter.ClientId);
            }
            query = query.Where(x => ids.Contains(x.ClientId));
        }
        else if (!String.IsNullOrWhiteSpace(filter.ClientId))
        {
            query = query.Where(x => x.ClientId == filter.ClientId);
        }

        if (!String.IsNullOrWhiteSpace(filter.SessionId))
        {
            query = query.Where(x => x.SessionId == filter.SessionId);
        }
        if (!String.IsNullOrWhiteSpace(filter.SubjectId))
        {
            query = query.Where(x => x.SubjectId == filter.SubjectId);
        }

        if (null != filter.Types)
        {
            var types = filter.Types.ToList();
            if (!String.IsNullOrWhiteSpace(filter.Type))
            {
                types.Add(filter.Type);
            }
            query = query.Where(x => types.Contains(x.Type));
        }
        else if (!String.IsNullOrWhiteSpace(filter.Type))
        {
            query = query.Where(x => x.Type == filter.Type);
        }

        return query;
    }
}