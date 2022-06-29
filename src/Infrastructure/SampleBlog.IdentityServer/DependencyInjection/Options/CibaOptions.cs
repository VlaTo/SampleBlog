namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Configures client initiated backchannel authentication
/// </summary>
public class CibaOptions
{
    /// <summary>
    /// Gets or sets the default lifetime of the request in seconds.
    /// </summary>
    public TimeSpan DefaultLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the polling interval in seconds.
    /// </summary>
    public TimeSpan DefaultPollingInterval
    {
        get;
        set;
    }

    public CibaOptions()
    {
        DefaultLifetime = TimeSpan.FromSeconds(300.0d);
        DefaultPollingInterval = TimeSpan.FromSeconds(5.0d);
    }
}