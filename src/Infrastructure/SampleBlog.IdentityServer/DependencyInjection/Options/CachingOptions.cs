namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Caching options.
/// </summary>
public class CachingOptions
{
    /// <summary>
    /// Gets or sets the client store expiration.
    /// </summary>
    /// <value>
    /// The client store expiration.
    /// </value>
    public TimeSpan ClientStoreExpiration
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the scope store expiration.
    /// </summary>
    /// <value>
    /// The scope store expiration.
    /// </value>
    public TimeSpan ResourceStoreExpiration
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the CORS origin expiration.
    /// </summary>
    public TimeSpan CorsExpiration
    {
        get;
        set;
    }

    /// <summary>
    /// Duration identity provider store cache duration
    /// </summary>
    public TimeSpan IdentityProviderCacheDuration
    {
        get;
        set;
    }


    /// <summary>
    /// The timeout for concurrency locking in the default cache.
    /// </summary>
    public TimeSpan CacheLockTimeout
    {
        get;
        set;
    }

    public CachingOptions()
    {
        var timeout = TimeSpan.FromMinutes(15);

        ClientStoreExpiration = timeout;
        ResourceStoreExpiration = timeout;
        CorsExpiration = timeout;
        IdentityProviderCacheDuration = TimeSpan.FromMinutes(60.0d);
        CacheLockTimeout = TimeSpan.FromSeconds(60.0d);
    }
}