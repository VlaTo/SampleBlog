namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Configures device flow
/// </summary>
public sealed class DeviceFlowOptions
{
    /// <summary>
    /// Gets or sets the default type of the user code.
    /// </summary>
    /// <value>
    /// The default type of the user code.
    /// </value>
    public string DefaultUserCodeType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the polling interval in seconds.
    /// </summary>
    /// <value>
    /// The interval in seconds.
    /// </value>
    public TimeSpan Interval
    {
        get;
        set;
    }

    public DeviceFlowOptions()
    {
        Interval = TimeSpan.FromSeconds(5.0d);
        DefaultUserCodeType = IdentityServerConstants.UserCodeTypes.Numeric;
    }
}