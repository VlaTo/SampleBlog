using IdentityModel;

namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Options for configuring logging behavior
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// 
    /// </summary>
    public ICollection<string> BackchannelAuthenticationRequestSensitiveValuesFilter
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public ICollection<string> TokenRequestSensitiveValuesFilter
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public ICollection<string> AuthorizeRequestSensitiveValuesFilter
    {
        get;
        set;
    }

    public LoggingOptions()
    {
        BackchannelAuthenticationRequestSensitiveValuesFilter = new HashSet<string>
        {
            // TODO: IdentityModel
            OidcConstants.TokenRequest.ClientSecret,
            OidcConstants.TokenRequest.ClientAssertion,
            OidcConstants.AuthorizeRequest.IdTokenHint
        };
        TokenRequestSensitiveValuesFilter = new HashSet<string>
        {
            OidcConstants.TokenRequest.ClientSecret,
            OidcConstants.TokenRequest.Password,
            OidcConstants.TokenRequest.ClientAssertion,
            OidcConstants.TokenRequest.RefreshToken,
            OidcConstants.TokenRequest.DeviceCode
        };
        AuthorizeRequestSensitiveValuesFilter = new HashSet<string>
        {
            OidcConstants.AuthorizeRequest.IdTokenHint
        };
    }
}