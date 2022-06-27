using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Default token service
/// </summary>
public class DefaultTokenService : ITokenService
{
    /// <summary>
    /// The HTTP context accessor
    /// </summary>
    protected IHttpContextAccessor ContextAccessor
    {
        get;
    }

    /// <summary>
    /// The claims provider
    /// </summary>
    protected IClaimsService ClaimsProvider
    {
        get;
    }

    /// <summary>
    /// The reference token store
    /// </summary>
    protected IReferenceTokenStore ReferenceTokenStore
    {
        get;
    }

    /// <summary>
    /// The signing service
    /// </summary>
    protected ITokenCreationService CreationService
    {
        get;
    }

    /// <summary>
    /// The clock
    /// </summary>
    protected ISystemClock Clock
    {
        get;
    }

    /// <summary>
    /// The key material service
    /// </summary>
    protected IKeyMaterialService KeyMaterialService
    {
        get;
    }

    /// <summary>
    /// The logger
    /// </summary>
    protected ILogger Logger
    {
        get;
    }

    /// <summary>
    /// The IdentityServer options
    /// </summary>
    protected IdentityServerOptions Options
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTokenService" /> class.
    /// </summary>
    /// <param name="claimsProvider">The claims provider.</param>
    /// <param name="referenceTokenStore">The reference token store.</param>
    /// <param name="creationService">The signing service.</param>
    /// <param name="contextAccessor">The HTTP context accessor.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="keyMaterialService"></param>
    /// <param name="options">The IdentityServer options</param>
    /// <param name="logger">The logger.</param>
    public DefaultTokenService(
        IClaimsService claimsProvider,
        IReferenceTokenStore referenceTokenStore,
        ITokenCreationService creationService,
        IHttpContextAccessor contextAccessor,
        ISystemClock clock,
        IKeyMaterialService keyMaterialService,
        IdentityServerOptions options,
        ILogger<DefaultTokenService> logger)
    {
        ContextAccessor = contextAccessor;
        ClaimsProvider = claimsProvider;
        ReferenceTokenStore = referenceTokenStore;
        CreationService = creationService;
        Clock = clock;
        KeyMaterialService = keyMaterialService;
        Options = options;
        Logger = logger;
    }

    /// <summary>
    /// Creates an identity token.
    /// </summary>
    /// <param name="request">The token creation request.</param>
    /// <returns>
    /// An identity token
    /// </returns>
    public virtual async Task<Token> CreateIdentityTokenAsync(TokenCreationRequest request)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultTokenService.CreateIdentityToken");

        Logger.LogTrace("Creating identity token");
        request.Validate();

        // todo: Dom, add a test for this. validate the at and c hashes are correct for the id_token when the client's alg doesn't match the server default.
        var credential = await KeyMaterialService.GetSigningCredentialsAsync(
            request.ValidatedRequest.Client?.AllowedIdentityTokenSigningAlgorithms
        );

        if (null == credential)
        {
            throw new InvalidOperationException("No signing credential is configured.");
        }

        // host provided claims
        var claims = new List<Claim>();
        var signingAlgorithm = credential.Algorithm;

        // if nonce was sent, must be mirrored in id token
        if (false == String.IsNullOrEmpty(request.Nonce))
        {
            claims.Add(new Claim(JwtClaimTypes.Nonce, request.Nonce));
        }

        // add at_hash claim
        if (request.AccessTokenToHash.IsPresent())
        {
            var hash = CryptoHelper.CreateHashClaimValue(request.AccessTokenToHash, signingAlgorithm);
            claims.Add(new Claim(JwtClaimTypes.AccessTokenHash, hash));
        }

        // add c_hash claim
        if (request.AuthorizationCodeToHash.IsPresent())
        {
            var hash = CryptoHelper.CreateHashClaimValue(request.AuthorizationCodeToHash, signingAlgorithm);
            claims.Add(new Claim(JwtClaimTypes.AuthorizationCodeHash, hash));
        }

        // add s_hash claim
        if (false == String.IsNullOrEmpty(request.StateHash))
        {
            claims.Add(new Claim(JwtClaimTypes.StateHash, request.StateHash));
        }

        // add sid if present
        if (false == String.IsNullOrEmpty(request.ValidatedRequest.SessionId))
        {
            claims.Add(new Claim(JwtClaimTypes.SessionId, request.ValidatedRequest.SessionId));
        }

        var identityTokenClaims = await ClaimsProvider.GetIdentityTokenClaimsAsync(
            request.Subject!,
            request.ValidatedResources,
            request.IncludeAllIdentityClaims,
            request.ValidatedRequest);

        claims.AddRange(identityTokenClaims);

        var issuer = request.ValidatedRequest.IssuerName;
        var token = new Token(OidcConstants.TokenTypes.IdentityToken)
        {
            CreationTime = Clock.UtcNow.UtcDateTime,
            Audiences = { request.ValidatedRequest.Client.ClientId },
            Issuer = issuer!,
            Lifetime = request.ValidatedRequest.Client.IdentityTokenLifetime,
            Claims = claims.Distinct(new ClaimComparer()).ToList(),
            ClientId = request.ValidatedRequest.Client.ClientId,
            AccessTokenType = request.ValidatedRequest.AccessTokenType,
            AllowedSigningAlgorithms = request.ValidatedRequest.Client.AllowedIdentityTokenSigningAlgorithms
        };

        return token;
    }

    /// <summary>
    /// Creates an access token.
    /// </summary>
    /// <param name="request">The token creation request.</param>
    /// <returns>
    /// An access token
    /// </returns>
    public virtual async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultTokenService.CreateAccessToken");

        Logger.LogTrace("Creating access token");
        request.Validate();

        var claims = new List<Claim>();
        var accessTokenClaims = await ClaimsProvider.GetAccessTokenClaimsAsync(
            request.Subject!,
            request.ValidatedResources,
            request.ValidatedRequest
        );

        claims.AddRange(accessTokenClaims);

        if (false == String.IsNullOrEmpty(request.ValidatedRequest.SessionId))
        {
            claims.Add(new Claim(JwtClaimTypes.SessionId, request.ValidatedRequest.SessionId));
        }

        var issuer = request.ValidatedRequest.IssuerName;
        var token = new Token(OidcConstants.TokenTypes.AccessToken)
        {
            CreationTime = Clock.UtcNow.UtcDateTime,
            Issuer = issuer!,
            Lifetime = request.ValidatedRequest.AccessTokenLifetime,
            IncludeJwtId = request.ValidatedRequest.Client!.IncludeJwtId,
            Claims = claims.Distinct(new ClaimComparer()).ToList(),
            ClientId = request.ValidatedRequest.Client.ClientId,
            Description = request.Description,
            AccessTokenType = request.ValidatedRequest.AccessTokenType,
            AllowedSigningAlgorithms = request.ValidatedResources.Resources.ApiResources.FindMatchingSigningAlgorithms()
        };

        // add aud based on ApiResources in the validated request
        var audiences = request.ValidatedResources.Resources.ApiResources
            .Select(x => x.Name)
            .Distinct();

        foreach (var aud in audiences)
        {
            token.Audiences.Add(aud);
        }

        if (Options.EmitStaticAudienceClaim)
        {
            token.Audiences.Add(
                String.Format(IdentityServerConstants.AccessTokenAudience, issuer.EnsureTrailingSlash())
            );
        }

        // add cnf if present
        if (request.ValidatedRequest.Confirmation.IsPresent())
        {
            token.Confirmation = request.ValidatedRequest.Confirmation;
        }
        else
        {
            if (Options.MutualTls.AlwaysEmitConfirmationClaim && null != ContextAccessor.HttpContext?.Connection)
            {
                var clientCertificate = await ContextAccessor.HttpContext.Connection.GetClientCertificateAsync();

                if (null != clientCertificate)
                {
                    token.Confirmation = clientCertificate.CreateThumbprintCnf();
                }
            }
        }

        return token;
    }

    /// <summary>
    /// Creates a serialized and protected security token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>
    /// A security token in serialized form
    /// </returns>
    /// <exception cref="System.InvalidOperationException">Invalid token type.</exception>
    public async Task<string> CreateSecurityTokenAsync(Token token)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultTokenService.CreateSecurityToken");

        string tokenResult;

        if (OidcConstants.TokenTypes.AccessToken == token.Type)
        {
            var currentJwtId = token.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.JwtId);

            if (token.IncludeJwtId || (null != currentJwtId && 5 > token.Version))
            {
                if (null != currentJwtId)
                {
                    token.Claims.Remove(currentJwtId);
                }

                token.Claims.Add(new Claim(
                    JwtClaimTypes.JwtId,
                    CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex)
                ));
            }

            if (AccessTokenType.Jwt == token.AccessTokenType)
            {
                Logger.LogTrace("Creating JWT access token");

                tokenResult = await CreationService.CreateTokenAsync(token);
            }
            else
            {
                Logger.LogTrace("Creating reference access token");

                var handle = await ReferenceTokenStore.StoreReferenceTokenAsync(token);

                tokenResult = handle;
            }
        }
        else if (OidcConstants.TokenTypes.IdentityToken == token.Type)
        {
            Logger.LogTrace("Creating JWT identity token");

            tokenResult = await CreationService.CreateTokenAsync(token);
        }
        else
        {
            throw new InvalidOperationException("Invalid token type.");
        }

        return tokenResult;
    }
}