using IdentityModel;

namespace SampleBlog.IdentityServer.ResponseHandling.Models;

/// <summary>
/// Models a token error response
/// </summary>
public class TokenErrorResponse
{
    /// <summary>
    /// Gets or sets the error.
    /// </summary>
    /// <value>
    /// The error.
    /// </value>
    public string Error
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the error description.
    /// </summary>
    /// <value>
    /// The error description.
    /// </value>
    public string? ErrorDescription
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the custom entries.
    /// </summary>
    /// <value>
    /// The custom.
    /// </value>
    public Dictionary<string, object>? Custom
    {
        get;
        set;
    }

    public TokenErrorResponse()
    {
        Error = OidcConstants.TokenErrors.InvalidRequest;
        Custom = new Dictionary<string, object>();
    }
}