using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// The backchannel authentication request id validator
/// </summary>
public interface IBackChannelAuthenticationRequestIdValidator
{
    /// <summary>
    /// Validates the authentication request id.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task ValidateAsync(BackchannelAuthenticationRequestIdValidationContext context);
}