using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Services;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Stores;

/// <summary>
/// Implementation of IResourceStore thats uses EF.
/// </summary>
/// <seealso cref="IResourceStore" />
public class ResourceStore : IResourceStore
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
    protected readonly ILogger<ResourceStore> Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceStore"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationTokenProvider"></param>
    /// <exception cref="ArgumentNullException">context</exception>
    public ResourceStore(
        IConfigurationDbContext context,
        ILogger<ResourceStore> logger,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = logger;
        CancellationTokenProvider = cancellationTokenProvider;
    }

    /// <summary>
    /// Finds the API resources by name.
    /// </summary>
    /// <param name="apiResourceNames">The names.</param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ResourceStore.FindApiResourcesByName");
        //activity?.SetTag(Tracing.Properties.ApiResourceNames, apiResourceNames.ToSpaceSeparatedString());

        //if (apiResourceNames == null) throw new ArgumentNullException(nameof(apiResourceNames));

        /*var query =
            from apiResource in Context.ApiResources
            where apiResourceNames.Contains(apiResource.Name)
            select apiResource;

        var apis = query
            .Include(x => x.Secrets)
            .Include(x => x.Scopes)
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();
        var result = (await apis.ToArrayAsync(CancellationTokenProvider.CancellationToken))
            .Where(x => apiResourceNames.Contains(x.Name))
            .Select(x => x.ToModel()).ToArray();
        */

        var query = Context.ApiResources
            .Where(resource => apiResourceNames.Contains(resource.Name))
            .Include(resource => resource.Secrets)
            .Include(resource => resource.Scopes)
            .Include(resource => resource.UserClaims)
            .Include(resource => resource.Properties)
            .AsNoTracking();

        var result = await query.ToArrayAsync(CancellationTokenProvider.CancellationToken);
        var resources = result
            .Select(resource => resource.ToModel())
            .ToArray();

        if (0 < resources.Length)
        {
            Logger.LogDebug("Found {apis} API resource in database", result.Select(x => x.Name));
        }
        else
        {
            Logger.LogDebug("Did not find {apis} API resource in database", apiResourceNames);
        }

        return resources;
    }

    public async Task<Resources> GetAllResourcesAsync()
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ResourceStore.GetAllResources");

        var identity = await Context.IdentityResources
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        var apis = await Context.ApiResources
            .Include(x => x.Secrets)
            .Include(x => x.Scopes)
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        var scopes = await Context.ApiScopes
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        var result = Resources.Create(
            identity.Select(resource => resource.ToModel()),
            apis.Select(resource => resource.ToModel()),
            scopes.Select(resource => resource.ToModel())
        );

        Logger.LogDebug("Found {scopes} as all scopes, and {apis} as API resources",
            result.IdentityResources
                .Select(x => x.Name)
                .Union(result.ApiScopes.Select(x => x.Name)),
            result.ApiResources.Select(x => x.Name)
        );

        return result;
    }

    /// <summary>
    /// Gets identity resources by scope name.
    /// </summary>
    /// <param name="scopeNames"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ResourceStore.FindIdentityResourcesByScopeName");
        //activity?.SetTag(Tracing.Properties.ScopeNames, scopeNames.ToSpaceSeparatedString());

        var scopes = scopeNames.ToArray();

        var query =
            from identityResource in Context.IdentityResources
            where scopes.Contains(identityResource.Name)
            select identityResource;

        var resources = query
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var results = (await resources.ToArrayAsync(CancellationTokenProvider.CancellationToken))
            .Where(x => scopes.Contains(x.Name));

        Logger.LogDebug("Found {scopes} identity scopes in database", results.Select(x => x.Name));

        return results
            .Select(x => x.ToModel())
            .ToArray();
    }

    public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ResourceStore.FindApiScopesByName");
        //activity?.SetTag(Tracing.Properties.ScopeNames, scopeNames.ToSpaceSeparatedString());

        var scopes = scopeNames.ToArray();

        var query =
            from scope in Context.ApiScopes
            where scopes.Contains(scope.Name)
            select scope;

        var resources = query
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var results = (await resources.ToArrayAsync(CancellationTokenProvider.CancellationToken))
            .Where(x => scopes.Contains(x.Name));

        Logger.LogDebug("Found {scopes} scopes in database", results.Select(x => x.Name));

        return results
            .Select(x => x.ToModel())
            .ToArray();
    }

    /// <summary>
    /// Gets API resources by scope name.
    /// </summary>
    /// <param name="scopeNames"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ResourceStore.FindApiResourcesByScopeName");
        //activity?.SetTag(Tracing.Properties.ScopeNames, scopeNames.ToSpaceSeparatedString());

        var names = scopeNames.ToArray();

        var query =
            from api in Context.ApiResources
            where api.Scopes.Any(x => names.Contains(x.Scope))
            select api;

        var apis = query
            .Include(x => x.Secrets)
            .Include(x => x.Scopes)
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var results = (await apis.ToArrayAsync(CancellationTokenProvider.CancellationToken))
            .Where(api => api.Scopes.Any(x => names.Contains(x.Scope)));
        var models = results.Select(x => x.ToModel()).ToArray();

        Logger.LogDebug("Found {apis} API resources in database", models.Select(x => x.Name));

        return models;
    }
}