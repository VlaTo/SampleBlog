using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.Extensions;

public static class ResourceStoreExtensions
{
    /// <summary>
    /// Finds the resources by scope.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="scopeNames">The scope names.</param>
    /// <returns></returns>
    public static async Task<Resources> FindResourcesByScopeAsync(this IResourceStore store, IEnumerable<string> scopeNames)
    {
        var identity = await store.FindIdentityResourcesByScopeNameAsync(scopeNames);
        var apiResources = await store.FindApiResourcesByScopeNameAsync(scopeNames);
        var scopes = await store.FindApiScopesByNameAsync(scopeNames);

        ValidateNameUniqueness(identity, apiResources, scopes);

        return Resources.Create(
            identity,
            apiResources,
            scopes,
            scopeNames.Contains(IdentityServerConstants.StandardScopes.OfflineAccess)
        );
    }

    /// <summary>
    /// Finds the enabled resources by scope.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="scopeNames">The scope names.</param>
    /// <returns></returns>
    public static async Task<Resources> FindEnabledResourcesByScopeAsync(this IResourceStore store, IEnumerable<string> scopeNames)
    {
        var resources = await store.FindResourcesByScopeAsync(scopeNames);
        return resources.FilterEnabled();
    }

    /// <summary>
    /// Finds the enabled identity resources by scope.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="scopeNames">The scope names.</param>
    /// <returns></returns>
    public static async Task<IReadOnlyCollection<IdentityResource>> FindEnabledIdentityResourcesByScopeAsync(this IResourceStore store, IEnumerable<string> scopeNames)
    {
        var identityResources = await store.FindIdentityResourcesByScopeNameAsync(scopeNames);
        return identityResources.Where(x => x.Enabled).ToArray();
    }

    private static void ValidateNameUniqueness(IEnumerable<IdentityResource> identity, IEnumerable<ApiResource> apiResources, IEnumerable<ApiScope> apiScopes)
    {
        // attempt to detect invalid configuration. this is about the only place
        // we can do this, since it's hard to get the values in the store.
        var identityScopeNames = GetDuplicates(identity.Select(x => x.Name).ToArray());
        //var dups = GetDuplicates(identityScopeNames);

        if (identityScopeNames.Any())
        {
            var names = identityScopeNames.Aggregate((x, y) => x + ", " + y);
            throw new Exception($"Duplicate identity scopes found. This is an invalid configuration. Use different names for identity scopes. Scopes found: {names}");
        }

        var apiNames = GetDuplicates(apiResources.Select(x => x.Name));
        //dups = GetDuplicates(apiNames);

        if (apiNames.Any())
        {
            var names = apiNames.Aggregate((x, y) => x + ", " + y);
            throw new Exception($"Duplicate api resources found. This is an invalid configuration. Use different names for API resources. Names found: {names}");
        }

        var scopesNames = GetDuplicates(apiScopes.Select(x => x.Name));
        //dups = GetDuplicates(scopesNames);

        if (scopesNames.Any())
        {
            var names = scopesNames.Aggregate((x, y) => x + ", " + y);
            throw new Exception($"Duplicate scopes found. This is an invalid configuration. Use different names for scopes. Names found: {names}");
        }

        var overlap = identityScopeNames.Intersect(scopesNames).ToArray();

        if (overlap.Any())
        {
            var names = overlap.Aggregate((x, y) => x + ", " + y);
            throw new Exception($"Found identity scopes and API scopes that use the same names. This is an invalid configuration. Use different names for identity scopes and API scopes. Scopes found: {names}");
        }
    }

    private static ICollection<string> GetDuplicates(IEnumerable<string> names)
    {
        return names
            .GroupBy(name => name)
            .Where(grouping => 1 < grouping.Count())
            .Select(grouping => grouping.Key)
            .ToArray();
    }
}