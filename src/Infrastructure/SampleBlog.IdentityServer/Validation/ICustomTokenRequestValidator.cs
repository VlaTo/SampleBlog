using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Default custom request validator
/// </summary>
public interface ICustomTokenRequestValidator
{
    /// <summary>
    /// Custom validation logic for a token request.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>
    /// The validation result
    /// </returns>
    Task ValidateAsync(CustomTokenRequestValidationContext context);
}