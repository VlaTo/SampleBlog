using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Options for aspects of the user interface.
/// </summary>
public class UserInteractionOptions
{
    /// <summary>
    /// Gets or sets the login URL. If a local URL, the value must start with a leading slash.
    /// </summary>
    /// <value>
    /// The login URL.
    /// </value>
    public string? LoginUrl
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the login return URL parameter.
    /// </summary>
    /// <value>
    /// The login return URL parameter.
    /// </value>
    public string? LoginReturnUrlParameter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the logout URL. If a local URL, the value must start with a leading slash.
    /// </summary>
    /// <value>
    /// The logout URL.
    /// </value>
    public string? LogoutUrl
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the logout identifier parameter.
    /// </summary>
    /// <value>
    /// The logout identifier parameter.
    /// </value>
    public string? LogoutIdParameter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the consent URL. If a local URL, the value must start with a leading slash.
    /// </summary>
    /// <value>
    /// The consent URL.
    /// </value>
    public string? ConsentUrl
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the consent return URL parameter.
    /// </summary>
    /// <value>
    /// The consent return URL parameter.
    /// </value>
    public string? ConsentReturnUrlParameter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the error URL. If a local URL, the value must start with a leading slash.
    /// </summary>
    /// <value>
    /// The error URL.
    /// </value>
    public string? ErrorUrl
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the error identifier parameter.
    /// </summary>
    /// <value>
    /// The error identifier parameter.
    /// </value>
    public string? ErrorIdParameter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the custom redirect return URL parameter.
    /// </summary>
    /// <value>
    /// The custom redirect return URL parameter.
    /// </value>
    public string? CustomRedirectReturnUrlParameter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the cookie message threshold. This limits how many cookies are created, and older ones will be purged.
    /// </summary>
    /// <value>
    /// The cookie message threshold.
    /// </value>
    public int CookieMessageThreshold
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the device verification URL.  If a local URL, the value must start with a leading slash.
    /// </summary>
    /// <value>
    /// The device verification URL.
    /// </value>
    public string? DeviceVerificationUrl
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the device verification user code parameter.
    /// </summary>
    /// <value>
    /// The device verification user code parameter.
    /// </value>
    public string? DeviceVerificationUserCodeParameter
    {
        get;
        set;
    }

    /// <summary>
    /// Flag that allows return URL validation to accept full URL that includes the IdentityServer origin. Defaults to false.
    /// </summary>
    public bool AllowOriginInReturnUrl
    {
        get;
        set;
    }

    public UserInteractionOptions()
    {
        LoginUrl = Constants.UIConstants.DefaultRoutePaths.Login.EnsureLeadingSlash();
        LogoutUrl = Constants.UIConstants.DefaultRoutePaths.Logout.EnsureLeadingSlash();
        ConsentUrl = Constants.UIConstants.DefaultRoutePaths.Consent.EnsureLeadingSlash();
        ErrorUrl = Constants.UIConstants.DefaultRoutePaths.Error.EnsureLeadingSlash();
        LoginReturnUrlParameter = Constants.UIConstants.DefaultRoutePathParams.Login;
        LogoutIdParameter = Constants.UIConstants.DefaultRoutePathParams.Logout;
        ConsentReturnUrlParameter = Constants.UIConstants.DefaultRoutePathParams.Consent;
        ErrorIdParameter = Constants.UIConstants.DefaultRoutePathParams.Error;
        CustomRedirectReturnUrlParameter = Constants.UIConstants.DefaultRoutePathParams.Custom;
        CookieMessageThreshold = Constants.UIConstants.CookieMessageThreshold;
        DeviceVerificationUrl = Constants.UIConstants.DefaultRoutePaths.DeviceVerification;
        DeviceVerificationUserCodeParameter = Constants.UIConstants.DefaultRoutePathParams.UserCode;
        AllowOriginInReturnUrl = false;
    }
}