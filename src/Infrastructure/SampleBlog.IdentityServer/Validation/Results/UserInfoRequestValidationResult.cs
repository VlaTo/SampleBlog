using System.Security.Claims;

namespace SampleBlog.IdentityServer.Validation.Results;

/// <summary>
/// Validation result for userinfo requests
/// </summary>
public sealed class UserInfoRequestValidationResult : ValidationResult
{
    /// <summary>
    /// Gets or sets the token validation result.
    /// </summary>
    /// <value>
    /// The token validation result.
    /// </value>
    public TokenValidationResult TokenValidationResult
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The subject.
    /// </value>
    public ClaimsPrincipal Subject
    {
        get;
        set;
    }
}