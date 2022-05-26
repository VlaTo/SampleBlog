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
        
        activity?.SetTag(Tracing.Properties.ApiResourceNames, apiResourceNames.ToSpaceSeparatedString());

        var names = apiResourceNames.ToArray();

        var result = await Context.ApiResources
            .Where(resource => names.Contains(resource.Name))
            .Include(resource => resource.Secrets)
            .Include(resource => resource.Scopes)
            .Include(resource => resource.UserClaims)
            .Include(resource => resource.Properties)
            .AsNoTracking()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

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

        Logger.LogDebug(
            "Found {scopes} as all scopes, and {apis} as API resources",
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
        
        activity?.SetTag(Tracing.Properties.ScopeNames, scopeNames.ToSpaceSeparatedString());

        var scopes = scopeNames.ToArray();

        var resources = await Context.IdentityResources
            .Where(resource => scopes.Contains(resource.Name))
            .Include(resource => resource.UserClaims)
            .Include(resource => resource.Properties)
            .AsNoTracking()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        Logger.LogDebug("Found {scopes} identity scopes in database", resources.Select(x => x.Name));

        return resources
            .Select(x => x.ToModel())
            .ToArray();
    }

    public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ResourceStore.FindApiScopesByName");
        
        activity?.SetTag(Tracing.Properties.ScopeNames, scopeNames.ToSpaceSeparatedString());

        var scopes = scopeNames.ToArray();

        var resources = await Context.ApiScopes
            .Where(resource => scopes.Contains(resource.Name))
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        Logger.LogDebug("Found {scopes} scopes in database", resources.Select(x => x.Name));

        return resources
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
        
        activity?.SetTag(Tracing.Properties.ScopeNames, scopeNames.ToSpaceSeparatedString());

        var names = scopeNames.ToArray();

        var apiResources = await Context.ApiResources
            .Where(resource => resource.Scopes.Any(scope => names.Contains(scope.Scope)))
            .Include(resource => resource.Secrets)
            .Include(resource => resource.Scopes)
            .Include(resource => resource.UserClaims)
            .Include(resource => resource.Properties)
            .AsNoTracking()
            .AsSplitQuery()
            .ToArrayAsync(CancellationTokenProvider.CancellationToken);

        var models = apiResources
            .Select(x => x.ToModel())
            .ToArray();

        Logger.LogDebug("Found {apis} API resources in database", models.Select(x => x.Name));

        return models;
    }
}