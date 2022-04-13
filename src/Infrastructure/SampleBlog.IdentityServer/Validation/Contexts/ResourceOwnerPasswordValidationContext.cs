using SampleBlog.IdentityServer.Validation.Requests;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation.Contexts;

/// <summary>
/// Class describing the resource owner password validation context
/// </summary>
public class ResourceOwnerPasswordValidationContext
{
    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    /// <value>
    /// The name of the user.
    /// </value>
    public string? UserName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>
    /// The password.
    /// </value>
    public string? Password
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the request.
    /// </summary>
    /// <value>
    /// The request.
    /// </value>
    public ValidatedTokenRequest Request
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the result.
    /// </summary>
    /// <value>
    /// The result.
    /// </value>
    public GrantValidationResult Result
    {
        get;
        set;
    }

    public ResourceOwnerPasswordValidationContext()
    {
        Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
    }
}