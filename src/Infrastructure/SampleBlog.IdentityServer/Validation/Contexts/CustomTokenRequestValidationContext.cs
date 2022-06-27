using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation.Contexts;

/// <summary>
/// Context class for custom token request validation
/// </summary>
public sealed class CustomTokenRequestValidationContext
{
    /// <summary>
    /// Gets or sets the result.
    /// </summary>
    /// <value>
    /// The result.
    /// </value>
    public TokenRequestValidationResult Result
    {
        get;
        set;
    }
}