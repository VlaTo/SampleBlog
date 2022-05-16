using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Default resource owner password validator (no implementation == not supported)
/// </summary>
/// <seealso cref="IResourceOwnerPasswordValidator" />
public class NotSupportedResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotSupportedResourceOwnerPasswordValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public NotSupportedResourceOwnerPasswordValidator(ILogger<NotSupportedResourceOwnerPasswordValidator> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Validates the resource owner password credential
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        context.SetResult(
            new GrantValidationResult(TokenRequestErrors.UnsupportedGrantType)
        );

        logger.LogInformation("Resource owner password credential type not supported. Configure an IResourceOwnerPasswordValidator.");

        return Task.CompletedTask;
    }
}