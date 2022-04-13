using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Default custom request validator
/// </summary>
public class DefaultCustomAuthorizeRequestValidator : ICustomAuthorizeRequestValidator
{
    public DefaultCustomAuthorizeRequestValidator()
    {
    }

    /// <summary>
    /// Custom validation logic for the authorize request.
    /// </summary>
    /// <param name="context">The context.</param>
    public Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
    {
        return Task.CompletedTask;
    }
}