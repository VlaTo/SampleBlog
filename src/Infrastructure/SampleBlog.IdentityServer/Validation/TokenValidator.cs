using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.Contexts;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using TokenValidationResult = SampleBlog.IdentityServer.Validation.Results.TokenValidationResult;

namespace SampleBlog.IdentityServer.Validation;

internal sealed class TokenValidator : ITokenValidator
{
    private readonly ILogger logger;
    private readonly IdentityServerOptions options;
    private readonly IIssuerNameService issuerNameService;
    private readonly IReferenceTokenStore referenceTokenStore;
    private readonly ICustomTokenValidator customValidator;
    private readonly IClientStore clients;
    private readonly IProfileService profile;
    private readonly IKeyMaterialService keysService;
    private readonly ISessionCoordinationService sessionCoordinationService;
    private readonly ISystemClock clock;
    //private readonly TokenValidationLog _log;

    public TokenValidator(
        IdentityServerOptions options,
        IIssuerNameService issuerNameService,
        IClientStore clients,
        IProfileService profile,
        IReferenceTokenStore referenceTokenStore,
        ICustomTokenValidator customValidator,
        IKeyMaterialService keysService,
        ISessionCoordinationService sessionCoordinationService,
        ISystemClock clock,
        ILogger<TokenValidator> logger)
    {
        this.options = options;
        this.issuerNameService = issuerNameService;
        this.clients = clients;
        this.profile = profile;
        this.referenceTokenStore = referenceTokenStore;
        this.customValidator = customValidator;
        this.keysService = keysService;
        this.sessionCoordinationService = sessionCoordinationService;
        this.clock = clock;
        this.logger = logger;

        //_log = new TokenValidationLog();
    }

    public async Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string? clientId = null, bool validateLifetime = true)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("TokenValidator.ValidateIdentityToken");

        logger.LogDebug("Start identity token validation");

        if (token.Length > options.InputLengthRestrictions.Jwt)
        {
            logger.LogError("JWT too long");
            return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        if (null == clientId)
        {
            clientId = GetClientIdFromJwt(token);

            if (clientId.IsMissing())
            {
                logger.LogError("No clientId supplied, can't find id in identity token.");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }
        }

        //_log.ClientId = clientId;
        //_log.ValidateLifetime = validateLifetime;

        var client = await clients.FindEnabledClientByIdAsync(clientId);

        if (null == client)
        {
            logger.LogError("Unknown or disabled client: {clientId}.", clientId);
            return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        //_log.ClientName = client.ClientName;
        logger.LogDebug("Client found: {clientId} / {clientName}", client.ClientId, client.ClientName);

        var keys = await keysService.GetValidationKeysAsync();
        var result = await ValidateJwtAsync(token, keys, audience: clientId, validateLifetime: validateLifetime);

        result.Client = client;

        if (result.IsError)
        {
            LogError("Error validating JWT");
            return result;
        }

        logger.LogDebug("Calling into custom token validator: {type}", customValidator.GetType().FullName);

        var customResult = await customValidator.ValidateIdentityTokenAsync(result);

        if (customResult.IsError)
        {
            LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
            return customResult;
        }

        //_log.Claims = customResult.Claims.ToClaimsDictionary();

        LogSuccess();
        
        return customResult;
    }

    public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string? expectedScope = null)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("TokenValidator.ValidateAccessToken");

        logger.LogTrace("Start access token validation");

        //_log.ExpectedScope = expectedScope;
        //_log.ValidateLifetime = true;

        TokenValidationResult result;

        if (token.Contains("."))
        {
            if (token.Length > options.InputLengthRestrictions.Jwt)
            {
                logger.LogError("JWT too long");

                return new TokenValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                    ErrorDescription = "Token too long"
                };
            }

            //_log.AccessTokenType = AccessTokenType.Jwt.ToString();
            var keys = await keysService.GetValidationKeysAsync();

            result = await ValidateJwtAsync(token, keys);
        }
        else
        {
            if (token.Length > options.InputLengthRestrictions.TokenHandle)
            {
                logger.LogError("token handle too long");

                return new TokenValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                    ErrorDescription = "Token too long"
                };
            }

            //_log.AccessTokenType = AccessTokenType.Reference.ToString();
            result = await ValidateReferenceAccessTokenAsync(token);
        }

        //_log.Claims = result.Claims.ToClaimsDictionary();

        if (result.IsError)
        {
            return result;
        }

        // make sure client is still active (if client_id claim is present)
        var clientClaim = result.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.ClientId);

        if (null != clientClaim)
        {
            var client = await clients.FindEnabledClientByIdAsync(clientClaim.Value);

            if (null == client)
            {
                logger.LogError("Client deleted or disabled: {clientId}", clientClaim.Value);

                result.IsError = true;
                result.Error = OidcConstants.ProtectedResourceErrors.InvalidToken;
                result.Claims = null;

                return result;
            }
        }

        // make sure user is still active (if sub claim is present)
        var subClaim = result.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);

        if (null != subClaim)
        {
            var principal = Principal.Create("tokenvalidator", result.Claims.ToArray());

            if (result.ReferenceTokenId.IsPresent())
            {
                principal.Identities.First().AddClaim(
                    new Claim(JwtClaimTypes.ReferenceTokenId, result.ReferenceTokenId)
                );
            }

            var isActiveCtx = new IsActiveContext(
                principal,
                result.Client,
                IdentityServerConstants.ProfileIsActiveCallers.AccessTokenValidation
            );

            await profile.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                logger.LogError("User marked as not active: {subject}", subClaim.Value);

                result.IsError = true;
                result.Error = OidcConstants.ProtectedResourceErrors.InvalidToken;
                result.Claims = null;

                return result;
            }

            var sub = subClaim.Value;
            var sid = principal.FindFirstValue("sid");

            if (null != sid)
            {
                var request = new SessionValidationRequest
                {
                    SubjectId = sub,
                    SessionId = sid,
                    Client = result.Client,
                    Type = SessionValidationType.AccessToken
                };

                var sessionResult = await sessionCoordinationService.ValidateSessionAsync(request);

                if (false == sessionResult)
                {
                    logger.LogError("Server-side session invalid for subject Id {subjectId} and session Id {sessionId}.", sub, sid);
                    return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
                }
            }
        }

        // check expected scope(s)
        if (expectedScope.IsPresent())
        {
            var scope = result.Claims.FirstOrDefault(
                c => c.Type == JwtClaimTypes.Scope && c.Value == expectedScope
            );

            if (null == scope)
            {
                LogError($"Checking for expected scope {expectedScope} failed");
                return Invalid(OidcConstants.ProtectedResourceErrors.InsufficientScope);
            }
        }

        //logger.LogDebug("Calling into custom token validator: {type}", _customValidator.GetType().FullName);
        var customResult = await customValidator.ValidateAccessTokenAsync(result);

        if (customResult.IsError)
        {
            LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
            return customResult;
        }

        // add claims again after custom validation
        //_log.Claims = customResult.Claims.ToClaimsDictionary();

        LogSuccess();

        return customResult;
    }

    private async Task<TokenValidationResult> ValidateJwtAsync(
        string jwtString,
        IEnumerable<SecurityKeyInfo> validationKeys,
        bool validateLifetime = true,
        string? audience = null)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("TokenValidator.ValidateJwt");

        var handler = new JsonWebTokenHandler();

        var parameters = new TokenValidationParameters
        {
            ValidIssuer = await issuerNameService.GetCurrentAsync(),
            IssuerSigningKeys = validationKeys.Select(k => k.Key),
            ValidateLifetime = validateLifetime
        };

        if (audience.IsPresent())
        {
            parameters.ValidAudience = audience;
        }
        else
        {
            parameters.ValidateAudience = false;

            // if no audience is specified, we make at least sure that it is an access token
            if (options.AccessTokenJwtType.IsPresent())
            {
                parameters.ValidTypes = new[]
                {
                    options.AccessTokenJwtType
                };
            }
        }

        var result = handler.ValidateToken(jwtString, parameters);

        if (false == result.IsValid)
        {
            if (result.Exception is SecurityTokenExpiredException expiredException)
            {
                logger.LogInformation(expiredException, "JWT token validation error: {exception}", expiredException.Message);
                return Invalid(OidcConstants.ProtectedResourceErrors.ExpiredToken);
            }

            logger.LogError(result.Exception, "JWT token validation error: {exception}", result.Exception.Message);
            return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        var id = result.ClaimsIdentity;

        // if access token contains an ID, log it
        var jwtId = id.FindFirst(JwtClaimTypes.JwtId);

        if (null != jwtId)
        {
            //_log.JwtId = jwtId.Value;
        }

        // load the client that belongs to the client_id claim
        Client? client = null;
        var clientId = id.FindFirst(JwtClaimTypes.ClientId);

        if (null != clientId)
        {
            client = await clients.FindEnabledClientByIdAsync(clientId.Value);

            if (null == client)
            {
                LogError($"Client deleted or disabled: {clientId}");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }
        }

        var claims = id.Claims.ToList();

        // check the scope format (array vs space delimited string)
        var scopes = claims.Where(c => c.Type == JwtClaimTypes.Scope).ToArray();

        if (scopes.Any())
        {
            foreach (var scope in scopes)
            {
                if (scope.Value.Contains(" "))
                {
                    claims.Remove(scope);

                    var values = scope.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var value in values)
                    {
                        claims.Add(new Claim(JwtClaimTypes.Scope, value));
                    }
                }
            }
        }

        return new TokenValidationResult
        {
            IsError = false,
            Claims = claims,
            Client = client,
            Jwt = jwtString
        };
    }

    private async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("TokenValidator.ValidateReferenceAccessToken");

        //_log.TokenHandle = tokenHandle;
        var token = await referenceTokenStore.GetReferenceTokenAsync(tokenHandle);

        if (null == token)
        {
            LogError("Invalid reference token.");
            return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        if (token.CreationTime.HasExceeded(token.Lifetime, clock.UtcNow.UtcDateTime))
        {
            LogError("Token expired.");

            await referenceTokenStore.RemoveReferenceTokenAsync(tokenHandle);
            return Invalid(OidcConstants.ProtectedResourceErrors.ExpiredToken);
        }

        // load the client that is defined in the token
        Client? client = null;

        if (null != token.ClientId)
        {
            client = await clients.FindEnabledClientByIdAsync(token.ClientId);
        }

        if (null == client)
        {
            LogError($"Client deleted or disabled: {token.ClientId}");
            return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        return new TokenValidationResult
        {
            IsError = false,
            Client = client,
            Claims = ReferenceTokenToClaims(token),
            ReferenceToken = token,
            ReferenceTokenId = tokenHandle
        };
    }

    private static IEnumerable<Claim> ReferenceTokenToClaims(Token token)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Issuer, token.Issuer),
            new(JwtClaimTypes.NotBefore, new DateTimeOffset(token.CreationTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtClaimTypes.IssuedAt, new DateTimeOffset(token.CreationTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtClaimTypes.Expiration, (new DateTimeOffset(token.CreationTime) + token.Lifetime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var aud in token.Audiences)
        {
            claims.Add(new Claim(JwtClaimTypes.Audience, aud));
        }

        claims.AddRange(token.Claims);

        return claims;
    }

    public string? GetClientIdFromJwt(string token)
    {
        try
        {
            var jwt = new JwtSecurityToken(token);
            var clientId = jwt.Audiences.FirstOrDefault();

            return clientId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Malformed JWT token: {exception}", ex.Message);
            return null;
        }
    }

    private static TokenValidationResult Invalid(string error)
    {
        return new TokenValidationResult
        {
            IsError = true,
            Error = error
        };
    }

    private void LogError(string message)
    {
        //logger.LogError(message + "\n{@logMessage}", _log);
    }

    private void LogSuccess()
    {
        //logger.LogDebug("Token validation success\n{@logMessage}", _log);
    }
}