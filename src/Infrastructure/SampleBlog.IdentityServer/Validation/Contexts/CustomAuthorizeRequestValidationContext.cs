using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Validation.Contexts;

/// <summary>
/// Context for custom authorize request validation.
/// </summary>
public class CustomAuthorizeRequestValidationContext
{
    /// <summary>
    /// The result of custom validation. 
    /// </summary>
    public AuthorizeRequestValidationResult Result
    {
        get;
        set;
    }
}