using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Allows inserting custom validation logic into authorize and token requests
/// </summary>
public interface ICustomAuthorizeRequestValidator
{
    /// <summary>
    /// Custom validation logic for the authorize request.
    /// </summary>
    /// <param name="context">The context.</param>
    Task ValidateAsync(CustomAuthorizeRequestValidationContext context);
}