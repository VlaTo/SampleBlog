using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Validation.Results;
using Secret = SampleBlog.IdentityServer.Storage.Models.Secret;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates a secret based on RS256 signed JWT token
/// </summary>
public class PrivateKeyJwtSecretValidator : ISecretValidator
{
    private readonly IIssuerNameService issuerNameService;
    private readonly IReplayCache replayCache;
    private readonly IServerUrls urls;
    private readonly IdentityServerOptions options;
    private readonly ILogger logger;

    private const string Purpose = nameof(PrivateKeyJwtSecretValidator);

    /// <summary>
    /// Instantiates an instance of private_key_jwt secret validator
    /// </summary>
    public PrivateKeyJwtSecretValidator(
        IIssuerNameService issuerNameService,
        IReplayCache replayCache,
        IServerUrls urls,
        IdentityServerOptions options,
        ILogger<PrivateKeyJwtSecretValidator> logger)
    {
        this.issuerNameService = issuerNameService;
        this.replayCache = replayCache;
        this.urls = urls;
        this.options = options;
        this.logger = logger;
    }

    /// <summary>
    /// Validates a secret
    /// </summary>
    /// <param name="secrets">The stored secrets.</param>
    /// <param name="parsedSecret">The received secret.</param>
    /// <returns>
    /// A validation result
    /// </returns>
    /// <exception cref="System.ArgumentException">ParsedSecret.Credential is not a JWT token</exception>
    public async Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
    {
        var fail = new SecretValidationResult { Success = false };
        var success = new SecretValidationResult { Success = true };

        if (parsedSecret.Type != IdentityServerConstants.ParsedSecretTypes.JwtBearer)
        {
            return fail;
        }

        if (false == (parsedSecret.Credential is string jwtTokenString))
        {
            logger.LogError("ParsedSecret.Credential is not a string.");
            return fail;
        }

        List<SecurityKey> trustedKeys;

        try
        {
            trustedKeys = await secrets.GetKeysAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not parse secrets");

            return fail;
        }

        if (!trustedKeys.Any())
        {
            logger.LogError("There are no keys available to validate client assertion.");

            return fail;
        }

        var validAudiences = new[]
        {
            // token endpoint URL
            String.Concat(urls.BaseUrl.EnsureTrailingSlash(), Constants.ProtocolRoutePaths.Token),
            // TODO: remove the issuer URL in a future major release?
            // issuer URL
            String.Concat((await issuerNameService.GetCurrentAsync()).EnsureTrailingSlash(), Constants.ProtocolRoutePaths.Token)
        }.Distinct();

        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = trustedKeys,
            ValidateIssuerSigningKey = true,

            ValidIssuer = parsedSecret.Id,
            ValidateIssuer = true,

            ValidAudiences = validAudiences,
            ValidateAudience = true,

            RequireSignedTokens = true,
            RequireExpirationTime = true,

            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var handler = new JsonWebTokenHandler
        {
            MaximumTokenSizeInBytes = options.InputLengthRestrictions.Jwt
        };

        var result = await handler.ValidateTokenAsync(jwtTokenString, tokenValidationParameters);

        if (false == result.IsValid)
        {
            logger.LogError(result.Exception, "JWT token validation error");

            return fail;
        }

        var jwtToken = (JsonWebToken)result.SecurityToken;

        if (jwtToken.Subject != jwtToken.Issuer)
        {
            logger.LogError("Both 'sub' and 'iss' in the client assertion token must have a value of client_id.");

            return fail;
        }

        var exp = jwtToken.ValidTo;

        if (exp == DateTime.MinValue)
        {
            logger.LogError("exp is missing.");

            return fail;
        }

        var jti = jwtToken.Id;

        if (jti.IsMissing())
        {
            logger.LogError("jti is missing.");

            return fail;
        }

        if (await replayCache.ExistsAsync(Purpose, jti))
        {
            logger.LogError("jti is found in replay cache. Possible replay attack.");

            return fail;
        }

        await replayCache.AddAsync(Purpose, jti, exp.AddMinutes(5));

        return success;
    }
}