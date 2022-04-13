using IdentityModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Results;
using System.Security.Claims;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates JWT authorization request objects
/// </summary>
public class JwtRequestValidator : IJwtRequestValidator
{
    private readonly string audienceUri;

    /// <summary>
    /// JWT handler
    /// </summary>
    protected JsonWebTokenHandler Handler;

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The options
    /// </summary>
    protected readonly IdentityServerOptions Options;

    /// <summary>
    /// The issuer name service
    /// </summary>
    protected readonly IIssuerNameService IssuerNameService;

    /// <summary>
    /// Instantiates an instance of private_key_jwt secret validator
    /// </summary>
    public JwtRequestValidator(
        IdentityServerOptions options,
        IIssuerNameService issuerNameService,
        ILogger<JwtRequestValidator> logger)
    {
        Options = options;
        IssuerNameService = issuerNameService;
        Logger = logger;

        Handler = new JsonWebTokenHandler
        {
            MaximumTokenSizeInBytes = options.InputLengthRestrictions.Jwt
        };
    }

    /// <summary>
    /// Instantiates an instance of private_key_jwt secret validator (used for testing)
    /// </summary>
    internal JwtRequestValidator(string audience, ILogger<JwtRequestValidator> logger)
    {
        audienceUri = audience;

        Logger = logger;
        Handler = new JsonWebTokenHandler();
    }

    /// <summary>
    /// The audience URI to use
    /// </summary>
    protected async Task<string> GetAudienceUri()
    {
        if (audienceUri.IsPresent())
        {
            return audienceUri;
        }

        return await IssuerNameService.GetCurrentAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<JwtRequestValidationResult> ValidateAsync(JwtRequestValidationContext context)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("JwtRequestValidator.Validate");

        if (null == context)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (null == context.Client)
        {
            throw new ArgumentNullException(nameof(context.Client));
        }

        if (String.IsNullOrWhiteSpace(context.JwtTokenString))
        {
            throw new ArgumentNullException(nameof(context.JwtTokenString));
        }

        var fail = new JwtRequestValidationResult
        {
            IsError = true
        };

        List<SecurityKey> trustedKeys;
        try
        {
            trustedKeys = await GetKeysAsync(context.Client);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Could not parse client secrets");
            return fail;
        }

        if (false == trustedKeys.Any())
        {
            Logger.LogError("There are no keys available to validate JWT.");
            return fail;
        }

        JsonWebToken jwtSecurityToken;

        try
        {
            jwtSecurityToken = await ValidateJwtAsync(context, trustedKeys);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "JWT token validation error");
            return fail;
        }

        if (jwtSecurityToken.TryGetPayloadValue<string>(OidcConstants.AuthorizeRequest.Request, out _) ||
            jwtSecurityToken.TryGetPayloadValue<string>(OidcConstants.AuthorizeRequest.RequestUri, out _))
        {
            Logger.LogError("JWT payload must not contain request or request_uri");
            return fail;
        }

        var payload = await ProcessPayloadAsync(context, jwtSecurityToken);

        Logger.LogDebug("JWT request object validation success.");

        return new JwtRequestValidationResult
        {
            IsError = false,
            Payload = payload
        };
    }

    /// <summary>
    /// Retrieves keys for a given client
    /// </summary>
    /// <param name="client">The client</param>
    /// <returns></returns>
    protected virtual Task<List<SecurityKey>> GetKeysAsync(Client client)
    {
        return client.ClientSecrets.GetKeysAsync();
    }

    /// <summary>
    /// Validates the JWT token
    /// </summary>
    protected virtual async Task<JsonWebToken> ValidateJwtAsync(JwtRequestValidationContext context, IEnumerable<SecurityKey> keys)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = keys,
            ValidateIssuerSigningKey = true,

            ValidIssuer = context.Client.ClientId,
            ValidateIssuer = true,

            ValidAudience = await GetAudienceUri(),
            ValidateAudience = true,

            RequireSignedTokens = true,
            RequireExpirationTime = true
        };

        var strictJarValidation = context.StrictJarValidation ?? Options.StrictJarValidation;

        if (strictJarValidation)
        {
            tokenValidationParameters.ValidTypes = new[] { JwtClaimTypes.JwtTypes.AuthorizationRequest };
        }

        var result = await Handler.ValidateTokenAsync(context.JwtTokenString, tokenValidationParameters);

        if (false == result.IsValid)
        {
            throw result.Exception;
        }

        return (JsonWebToken)result.SecurityToken;
    }

    /// <summary>
    /// Processes the JWT contents
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token">The JWT token</param>
    /// <returns></returns>
    protected virtual Task<List<Claim>> ProcessPayloadAsync(JwtRequestValidationContext context, JsonWebToken token)
    {
        // filter JWT validation values
        var filter = Constants.Filters.JwtRequestClaimTypesFilter.ToList();
        if (context.IncludeJti)
        {
            // don't filter out the jti claim
            filter.Remove(JwtClaimTypes.JwtId);
        }

        var filtered = token.Claims.Where(claim => !filter.Contains(claim.Type));
        return Task.FromResult(filtered.ToList());
    }
}