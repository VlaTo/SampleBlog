using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;
using SampleBlog.IdentityServer.Storage.Services;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Stores;

/// <summary>
/// Implementation of IClientStore thats uses EF.
/// </summary>
/// <seealso cref="IClientStore" />
public class ClientStore : IClientStore
{
    /// <summary>
    /// The DbContext.
    /// </summary>
    protected readonly IConfigurationDbContext Context;

    /// <summary>
    /// The CancellationToken provider.
    /// </summary>
    protected readonly ICancellationTokenProvider CancellationTokenProvider;

    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger<ClientStore> Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientStore"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationTokenProvider"></param>
    /// <exception cref="ArgumentNullException">context</exception>
    public ClientStore(
        IConfigurationDbContext context,
        ICancellationTokenProvider cancellationTokenProvider,
        ILogger<ClientStore> logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        CancellationTokenProvider = cancellationTokenProvider;
        Logger = logger;
    }

    /// <summary>
    /// Finds a client by id
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>
    /// The client
    /// </returns>
    public virtual async Task<IdentityServer.Storage.Models.Client?> FindClientByIdAsync(string clientId)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ClientStore.FindClientById");
        
        activity?.SetTag(Tracing.Properties.ClientId, clientId);

        var query = Context.Clients
            .Where(x => x.ClientId == clientId)
            .Include(x => x.AllowedCorsOrigins)
            .Include(x => x.AllowedGrantTypes)
            .Include(x => x.AllowedScopes)
            .Include(x => x.Claims)
            .Include(x => x.ClientSecrets)
            .Include(x => x.IdentityProviderRestrictions)
            .Include(x => x.PostLogoutRedirectUris)
            .Include(x => x.Properties)
            .Include(x => x.RedirectUris)
            .AsNoTracking()
            .AsSplitQuery();

        var client = await query.SingleOrDefaultAsync(CancellationTokenProvider.CancellationToken);

        if (null == client)
        {
            return null;
        }

        var model = client.ToModel();

        Logger.LogDebug("Client {clientId} found in database", clientId);

        return model;
    }
}