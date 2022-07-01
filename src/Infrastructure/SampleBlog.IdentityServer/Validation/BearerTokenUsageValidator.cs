using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates a request that uses a bearer token for authentication
/// </summary>
internal sealed class BearerTokenUsageValidator
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenUsageValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public BearerTokenUsageValidator(ILogger<BearerTokenUsageValidator> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task<BearerTokenUsageValidationResult> ValidateAsync(HttpContext context)
    {
        var result = ValidateAuthorizationHeader(context);

        if (result.TokenFound)
        {
            logger.LogDebug("Bearer token found in header");
            return result;
        }

        if (context.Request.HasApplicationFormContentType())
        {
            result = await ValidatePostBodyAsync(context);

            if (result.TokenFound)
            {
                logger.LogDebug("Bearer token found in body");
                return result;
            }
        }

        logger.LogDebug("Bearer token not found");

        return new BearerTokenUsageValidationResult();
    }

    /// <summary>
    /// Validates the authorization header.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public BearerTokenUsageValidationResult ValidateAuthorizationHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authorizationHeader.IsPresent())
        {
            var header = authorizationHeader.Trim();

            if (header.StartsWith(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer))
            {
                var value = header
                    .Substring(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer.Length)
                    .Trim();

                if (value.IsPresent())
                {
                    return new BearerTokenUsageValidationResult
                    {
                        TokenFound = true,
                        Token = value,
                        UsageType = BearerTokenUsageType.AuthorizationHeader
                    };
                }
            }
            else
            {
                logger.LogTrace("Unexpected header format: {header}", header);
            }
        }

        return new BearerTokenUsageValidationResult();
    }

    /// <summary>
    /// Validates the post body.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public static async Task<BearerTokenUsageValidationResult> ValidatePostBodyAsync(HttpContext context)
    {
        var form = await context.Request.ReadFormAsync();

        if (form.TryGetValue("access_token", out var values) && 0 < values.Count)
        {
            return new BearerTokenUsageValidationResult
            {
                TokenFound = true,
                Token = values[0],
                UsageType = BearerTokenUsageType.PostBody
            };
        }

        return new BearerTokenUsageValidationResult();
    }
}