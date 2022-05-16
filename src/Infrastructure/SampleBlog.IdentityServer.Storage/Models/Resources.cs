namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// Models a collection of identity and API resources.
/// </summary>
public class Resources
{
    /// <summary>
    /// Gets or sets a value indicating whether [offline access].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [offline access]; otherwise, <c>false</c>.
    /// </value>
    public bool OfflineAccess
    {
        init;
        get;
    }

    /// <summary>
    /// Gets or sets the identity resources.
    /// </summary>
    public ICollection<IdentityResource> IdentityResources
    {
        init;
        get;
    }

    /// <summary>
    /// Gets or sets the API resources.
    /// </summary>
    public ICollection<ApiResource> ApiResources
    {
        init;
        get;
    }

    /// <summary>
    /// Gets or sets the API scopes.
    /// </summary>
    public ICollection<ApiScope> ApiScopes
    {
        init;
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Resources"/> class.
    /// </summary>
    public Resources()
    {
        IdentityResources = new HashSet<IdentityResource>();
        ApiResources = new HashSet<ApiResource>();
        ApiScopes = new HashSet<ApiScope>();
        OfflineAccess = false;
    }

    internal Resources(
        IEnumerable<IdentityResource> identityResources,
        IEnumerable<ApiResource> apiResources,
        IEnumerable<ApiScope> apiScopes,
        bool offlineAccess)
    {
        IdentityResources = new HashSet<IdentityResource>(identityResources.ToArray());
        ApiResources = new HashSet<ApiResource>(apiResources.ToArray());
        ApiScopes = new HashSet<ApiScope>(apiScopes.ToArray());
        OfflineAccess = offlineAccess;
    }

    public static Resources Create(
        IEnumerable<IdentityResource>? identityResources,
        IEnumerable<ApiResource>? apiResources,
        IEnumerable<ApiScope>? apiScopes,
        bool offlineAccess = false)
    {
        return new Resources(
            identityResources ?? Enumerable.Empty<IdentityResource>(),
            apiResources ?? Enumerable.Empty<ApiResource>(),
            apiScopes ?? Enumerable.Empty<ApiScope>(),
            offlineAccess
        );
    }

    public static Resources Create(Resources other)
    {
        return new Resources(other.IdentityResources, other.ApiResources, other.ApiScopes, other.OfflineAccess);
    }
}