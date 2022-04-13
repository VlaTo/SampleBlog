using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Extensions;

public static class ResourceExtensions
{
    /// <summary>
    /// Converts to scope names.
    /// </summary>
    /// <param name="resources">The resources.</param>
    /// <returns></returns>
    public static IEnumerable<string> ToScopeNames(this Resources resources)
    {
        var names = resources.IdentityResources.Select(x => x.Name).ToList();

        names.AddRange(resources.ApiScopes.Select(x => x.Name));
        
        if (resources.OfflineAccess)
        {
            names.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
        }

        return names;
    }

    /// <summary>
    /// Finds the API resources that contain the scope.
    /// </summary>
    /// <param name="resources">The resources.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static IEnumerable<ApiResource> FindApiResourcesByScope(this Resources resources, string name)
    {
        return resources.ApiResources
            .Where(api => api.Scopes.Contains(name))
            .ToArray();
    }

    /// <summary>
    /// Finds the API scope.
    /// </summary>
    /// <param name="resources">The resources.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static ApiScope? FindApiScope(this Resources resources, string name)
    {
        return resources.ApiScopes.FirstOrDefault(scope => scope.Name == name);
    }

    /// <summary>
    /// Finds the IdentityResource that matches the scope.
    /// </summary>
    /// <param name="resources">The resources.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static IdentityResource? FindIdentityResourcesByScope(this Resources resources, string name)
    {
        return resources.IdentityResources.FirstOrDefault(id => id.Name == name);
    }
    
    internal static Resources FilterEnabled(this Resources? resources)
    {
        if (null == resources)
        {
            return new Resources();
        }

        return Resources.Create(
            resources.IdentityResources.Where(x => x.Enabled),
            resources.ApiResources.Where(x => x.Enabled),
            resources.ApiScopes.Where(x => x.Enabled),
            resources.OfflineAccess
        );
    }
}