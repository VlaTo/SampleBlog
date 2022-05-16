using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Requests;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates an extension grant request using the registered validators
/// </summary>
public sealed class ExtensionGrantValidator
{
    private readonly IEnumerable<IExtensionGrantValidator> validators;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionGrantValidator"/> class.
    /// </summary>
    /// <param name="validators">The validators.</param>
    /// <param name="logger">The logger.</param>
    public ExtensionGrantValidator(
        IEnumerable<IExtensionGrantValidator>? validators,
        ILogger<ExtensionGrantValidator> logger)
    {
        this.validators = validators ?? Enumerable.Empty<IExtensionGrantValidator>();
        this.logger = logger;
    }

    /// <summary>
    /// Gets the available grant types.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAvailableGrantTypes() => validators.Select(v => v.GrantType);

    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public async Task<GrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
    {
        var validator = validators.FirstOrDefault(x => x.GrantType.Equals(request.GrantType, StringComparison.Ordinal));

        if (null == validator)
        {
            logger.LogError("No validator found for grant type");
            return new GrantValidationResult(TokenRequestErrors.UnsupportedGrantType);
        }

        try
        {
            logger.LogTrace("Calling into custom grant validator: {type}", validator.GetType().FullName);

            var context = new ExtensionGrantValidationContext(request);

            await validator.ValidateAsync(context);

            return context.Result;
        }
        catch (Exception e)
        {
            logger.LogError(1, e, "Grant validation error: {message}", e.Message);
            return new GrantValidationResult(TokenRequestErrors.InvalidGrant);
        }
    }
}