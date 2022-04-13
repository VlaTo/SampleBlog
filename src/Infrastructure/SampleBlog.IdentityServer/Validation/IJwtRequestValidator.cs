using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Interface for request object validator
/// </summary>
public interface IJwtRequestValidator
{
    /// <summary>
    /// Validates a JWT request object
    /// </summary>
    Task<JwtRequestValidationResult> ValidateAsync(JwtRequestValidationContext context);
}