using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Services;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Stores;

/// <summary>
/// Implementation of ISigningKeyStore thats uses EF.
/// </summary>
/// <seealso cref="ISigningKeyStore" />
public class SigningKeyStore : ISigningKeyStore
{
    const string Use = "signing";

    /// <summary>
    /// The DbContext.
    /// </summary>
    protected IPersistedGrantDbContext Context
    {
        init;
        get;
    }

    /// <summary>
    /// The CancellationToken provider.
    /// </summary>
    protected ICancellationTokenProvider CancellationTokenProvider
    {
        init;
        get;
    }

    /// <summary>
    /// The logger.
    /// </summary>
    protected ILogger<SigningKeyStore> Logger
    {
        init;
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SigningKeyStore"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationTokenProvider"></param>
    /// <exception cref="ArgumentNullException">context</exception>
    public SigningKeyStore(
        IPersistedGrantDbContext context,
        ICancellationTokenProvider cancellationTokenProvider,
        ILogger<SigningKeyStore> logger)
    {
        Context = context;
        Logger = logger;
        CancellationTokenProvider = cancellationTokenProvider;
    }

    /// <summary>
    /// Loads all keys from store.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<SerializedKey>> LoadKeysAsync()
    {
        using var activity = Tracing.ActivitySource.StartActivity("SigningKeyStore.LoadKeys");

        var entities = await Context.Keys
            .Where(x => x.Use == Use)
            .AsNoTracking()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        return entities
            .Select(key => new SerializedKey
            {
                Id = key.Id,
                Created = key.Created,
                Version = key.Version,
                Algorithm = key.Algorithm,
                Data = key.Data,
                DataProtected = key.DataProtected,
                IsX509Certificate = key.IsX509Certificate
            })
            .ToArray();
    }

    /// <summary>
    /// Persists new key in store.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task StoreKeyAsync(SerializedKey key)
    {
        using var activity = Tracing.ActivitySource.StartActivity("SigningKeyStore.StoreKey");

        var entity = new Key
        {
            Id = key.Id,
            Use = Use,
            Created = key.Created,
            Version = key.Version,
            Algorithm = key.Algorithm,
            Data = key.Data,
            DataProtected = key.DataProtected,
            IsX509Certificate = key.IsX509Certificate
        };

        Context.Keys.Add(entity);

        return Context.SaveChangesAsync(CancellationTokenProvider.CancellationToken);
    }

    /// <summary>
    /// Deletes key from storage.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteKeyAsync(string id)
    {
        using var activity = Tracing.ActivitySource.StartActivity("SigningKeyStore.DeleteKey");

        var item = await Context.Keys
            .Where(x => x.Use == Use && x.Id == id)
            .FirstOrDefaultAsync(CancellationTokenProvider.CancellationToken);

        if (null != item)
        {
            try
            {
                Context.Keys.Remove(item);
                
                await Context.SaveChangesAsync(CancellationTokenProvider.CancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entity in ex.Entries)
                {
                    entity.State = EntityState.Detached;
                }

                // already deleted, so we can eat this exception
                Logger.LogDebug("Concurrency exception caught deleting key id {kid}", id);
            }
        }
    }
}