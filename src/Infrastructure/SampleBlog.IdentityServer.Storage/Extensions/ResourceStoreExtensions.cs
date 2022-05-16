using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.Storage.Extensions;

public static class ResourceStoreExtensions
{
    /// <summary>
    /// Gets all enabled resources.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <returns></returns>
    public static async Task<Resources> GetAllEnabledResourcesAsync(this IResourceStore store)
    {
        var resources = await store.GetAllResourcesAsync();

        ValidateNameUniqueness(resources.IdentityResources, resources.ApiResources, resources.ApiScopes);

        return resources.FilterEnabled();
    }

    private static void ValidateNameUniqueness(IEnumerable<IdentityResource> identity, IEnumerable<ApiResource> apiResources, IEnumerable<ApiScope> apiScopes)
    {
        // attempt to detect invalid configuration. this is about the only place
        // we can do this, since it's hard to get the values in the store.
        var identityScopeNames = identity.Select(x => x.Name).ToArray();
        var dups = GetDuplicates(identityScopeNames);

        if (dups.Any())
        {
            var names = dups.Aggregate((x, y) => x + ", " + y);
            throw new Exception(
                $"Duplicate identity scopes found. This is an invalid configuration. Use different names for identity scopes. Scopes found: {names}");
        }

        var apiNames = apiResources.Select(x => x.Name);
    
        dups = GetDuplicates(apiNames);
        
        if (dups.Any())
        {
            var names = dups.Aggregate((x, y) => x + ", " + y);
            throw new Exception(
                $"Duplicate api resources found. This is an invalid configuration. Use different names for API resources. Names found: {names}");
        }

        var scopesNames = apiScopes.Select(x => x.Name);
        
        dups = GetDuplicates(scopesNames);
        
        if (dups.Any())
        {
            var names = dups.Aggregate((x, y) => x + ", " + y);
            throw new Exception(
                $"Duplicate scopes found. This is an invalid configuration. Use different names for scopes. Names found: {names}");
        }

        var overlap = identityScopeNames.Intersect(scopesNames).ToArray();
        
        if (overlap.Any())
        {
            var names = overlap.Aggregate((x, y) => x + ", " + y);
            throw new Exception(
                $"Found identity scopes and API scopes that use the same names. This is an invalid configuration. Use different names for identity scopes and API scopes. Scopes found: {names}");
        }
    }

    private static IEnumerable<string> GetDuplicates(IEnumerable<string> names)
    {
        var duplicates = names
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(y => y.Key)
            .ToArray();
        return duplicates.ToArray();
    }
}