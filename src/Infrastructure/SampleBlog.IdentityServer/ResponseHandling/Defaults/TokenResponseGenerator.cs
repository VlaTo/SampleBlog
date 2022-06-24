using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation;
using SampleBlog.IdentityServer.Validation.Requests;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.ResponseHandling.Defaults;

/// <summary>
/// The default token response generator
/// </summary>
/// <seealso cref="ITokenResponseGenerator" />
public class TokenResponseGenerator : ITokenResponseGenerator
{
    /// <summary>
    /// The token service
    /// </summary>
    protected ITokenService TokenService
    {
        get;
        init;
    }

    /// <summary>
    /// The refresh token service
    /// </summary>
    protected IRefreshTokenService RefreshTokenService
    {
        get;
        init;
    }

    /// <summary>
    /// The scope parser
    /// </summary>
    public IScopeParser ScopeParser
    {
        get;
    }

    /// <summary>
    /// The resource store
    /// </summary>
    protected IResourceStore Resources
    {
        get;
        init;
    }

    /// <summary>
    /// The clients store
    /// </summary>
    protected IClientStore Clients
    {
        get;
        init;
    }

    /// <summary>
    ///  The clock
    /// </summary>
    protected ISystemClock Clock
    {
        get;
        init;
    }

    /// <summary>
    /// The logger
    /// </summary>
    protected ILogger Logger
    {
        get;
        init;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenResponseGenerator" /> class.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <param name="tokenService">The token service.</param>
    /// <param name="refreshTokenService">The refresh token service.</param>
    /// <param name="scopeParser">The scope parser.</param>
    /// <param name="resources">The resources.</param>
    /// <param name="clients">The clients.</param>
    /// <param name="logger">The logger.</param>
    public TokenResponseGenerator(
        ISystemClock clock,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IScopeParser scopeParser,
        IResourceStore resources,
        IClientStore clients,
        ILogger<TokenResponseGenerator> logger)
    {
        Clock = clock;
        TokenService = tokenService;
        RefreshTokenService = refreshTokenService;
        ScopeParser = scopeParser;
        Resources = resources;
        Clients = clients;
        Logger = logger;
    }

    /// <summary>
    /// Processes the response.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public virtual async Task<TokenResponse> ProcessAsync(TokenRequestValidationResult request)
    {
        using var activity = Tracing.ActivitySource.StartActivity("TokenResponseGenerator.Process");

        activity?.SetTag(Tracing.Properties.GrantType, request.ValidatedRequest.GrantType);
        activity?.SetTag(Tracing.Properties.ClientId, request.ValidatedRequest.Client.ClientId);

        switch (request.ValidatedRequest.GrantType)
        {
            case OidcConstants.GrantTypes.ClientCredentials:
            {
                return await ProcessClientCredentialsRequestAsync(request);
            }

            case OidcConstants.GrantTypes.Password:
            {
                return await ProcessPasswordRequestAsync(request);
            }

            case OidcConstants.GrantTypes.AuthorizationCode:
            {
                return await ProcessAuthorizationCodeRequestAsync(request);
            }

            case OidcConstants.GrantTypes.RefreshToken:
            {
                return await ProcessRefreshTokenRequestAsync(request);
            }

            case OidcConstants.GrantTypes.DeviceCode:
            {
                return await ProcessDeviceCodeRequestAsync(request);
            }

            case OidcConstants.GrantTypes.Ciba:
            {
                return await ProcessCibaRequestAsync(request);
            }

            default:
            {
                return await ProcessExtensionGrantRequestAsync(request);
            }
        }
    }

    /// <summary>
    /// Creates the response for an client credentials request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected virtual Task<TokenResponse> ProcessClientCredentialsRequestAsync(TokenRequestValidationResult request)
    {
        Logger.LogTrace("Creating response for client credentials request");

        return ProcessTokenRequestAsync(request);
    }

    /// <summary>
    /// Creates the response for a password request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected virtual Task<TokenResponse> ProcessPasswordRequestAsync(TokenRequestValidationResult request)
    {
        Logger.LogTrace("Creating response for password request");

        return ProcessTokenRequestAsync(request);
    }

    /// <summary>
    /// Creates the response for an authorization code request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">Client does not exist anymore.</exception>
    protected virtual async Task<TokenResponse> ProcessAuthorizationCodeRequestAsync(TokenRequestValidationResult request)
    {
        Logger.LogTrace("Creating response for authorization code request");

        //////////////////////////
        // access token
        /////////////////////////
        var (accessToken, refreshToken) = await CreateAccessTokenAsync(request.ValidatedRequest);

        var response = new TokenResponse
        {
            AccessToken = accessToken,
            AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
            Custom = request.CustomResponse,
            Scope = request.ValidatedRequest.ValidatedResources.RawScopeValues.ToSpaceSeparatedString()
        };

        //////////////////////////
        // refresh token
        /////////////////////////
        if (refreshToken.IsPresent())
        {
            response.RefreshToken = refreshToken;
        }

        //////////////////////////
        // id token
        /////////////////////////
        if (request.ValidatedRequest.AuthorizationCode.IsOpenId)
        {
            // load the client that belongs to the authorization code
            Client? client = null;

            if (null != request.ValidatedRequest.AuthorizationCode.ClientId)
            {
                // todo: do we need this check?
                client = await Clients.FindEnabledClientByIdAsync(request.ValidatedRequest.AuthorizationCode.ClientId);
            }

            if (null == client)
            {
                throw new InvalidOperationException("Client does not exist anymore.");
            }

            var tokenRequest = new TokenCreationRequest
            {
                Subject = request.ValidatedRequest.AuthorizationCode.Subject,
                ValidatedResources = request.ValidatedRequest.ValidatedResources,
                Nonce = request.ValidatedRequest.AuthorizationCode.Nonce,
                AccessTokenToHash = response.AccessToken,
                StateHash = request.ValidatedRequest.AuthorizationCode.StateHash,
                ValidatedRequest = request.ValidatedRequest
            };

            var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest);
            var jwt = await TokenService.CreateSecurityTokenAsync(idToken);

            response.IdentityToken = jwt;
        }

        return response;
    }

    /// <summary>
    /// Creates the response for a refresh token request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected virtual async Task<TokenResponse> ProcessRefreshTokenRequestAsync(TokenRequestValidationResult request)
    {
        Logger.LogTrace("Creating response for refresh token request");

        var accessToken = request.ValidatedRequest.RefreshToken.GetAccessToken(request.ValidatedRequest.RequestedResourceIndicator);
        var mustUpdate = null == accessToken || request.ValidatedRequest.Client.UpdateAccessTokenClaimsOnRefresh;

        if (mustUpdate)
        {
            var creationRequest = new TokenCreationRequest
            {
                Subject = request.ValidatedRequest.RefreshToken.Subject,
                Description = request.ValidatedRequest.RefreshToken.Description,
                ValidatedRequest = request.ValidatedRequest,
                ValidatedResources = request.ValidatedRequest.ValidatedResources
            };
            accessToken = await TokenService.CreateAccessTokenAsync(creationRequest);
        }
        else
        {
            // todo: do we want a new JTI?
            accessToken.CreationTime = Clock.UtcNow.UtcDateTime;
            accessToken.Lifetime = request.ValidatedRequest.AccessTokenLifetime;
        }

        var accessTokenString = await TokenService.CreateSecurityTokenAsync(accessToken);

        request.ValidatedRequest.RefreshToken.SetAccessToken(accessToken, request.ValidatedRequest.RequestedResourceIndicator);

        var handle = await RefreshTokenService.UpdateRefreshTokenAsync(new RefreshTokenUpdateRequest
        {
            Handle = request.ValidatedRequest.RefreshTokenHandle,
            RefreshToken = request.ValidatedRequest.RefreshToken,
            Client = request.ValidatedRequest.Client,
            MustUpdate = mustUpdate
        });

        return new TokenResponse
        {
            IdentityToken = await CreateIdTokenFromRefreshTokenRequestAsync(request.ValidatedRequest, accessTokenString),
            AccessToken = accessTokenString,
            AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
            RefreshToken = handle,
            Custom = request.CustomResponse,
            Scope = request.ValidatedRequest.ValidatedResources.RawScopeValues.ToSpaceSeparatedString()
        };
    }

    /// <summary>
    /// Processes the response for device code grant request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected virtual async Task<TokenResponse> ProcessDeviceCodeRequestAsync(TokenRequestValidationResult request)
    {
        Logger.LogTrace("Creating response for device code request");

        //////////////////////////
        // access token
        /////////////////////////
        var (accessToken, refreshToken) = await CreateAccessTokenAsync(request.ValidatedRequest);

        var response = new TokenResponse
        {
            AccessToken = accessToken,
            AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
            Custom = request.CustomResponse,
            Scope = request.ValidatedRequest.ValidatedResources.RawScopeValues.ToSpaceSeparatedString()
        };

        //////////////////////////
        // refresh token
        /////////////////////////
        if (refreshToken.IsPresent())
        {
            response.RefreshToken = refreshToken;
        }

        //////////////////////////
        // id token
        /////////////////////////
        if (request.ValidatedRequest.DeviceCode.IsOpenId)
        {
            // load the client that belongs to the device code
            Client? client = null;

            if (null != request.ValidatedRequest.DeviceCode.ClientId)
            {
                // todo: do we need this check?
                client = await Clients.FindEnabledClientByIdAsync(request.ValidatedRequest.DeviceCode.ClientId);
            }

            if (null == client)
            {
                throw new InvalidOperationException("Client does not exist anymore.");
            }

            var tokenRequest = new TokenCreationRequest
            {
                Subject = request.ValidatedRequest.DeviceCode.Subject,
                AccessTokenToHash = response.AccessToken,
                ValidatedResources = request.ValidatedRequest.ValidatedResources,
                ValidatedRequest = request.ValidatedRequest
            };

            var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest);
            var jwt = await TokenService.CreateSecurityTokenAsync(idToken);
            
            response.IdentityToken = jwt;
        }

        return response;
    }

    /// <summary>
    /// Processes the response for CIBA request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected virtual async Task<TokenResponse> ProcessCibaRequestAsync(TokenRequestValidationResult request)
    {
        Logger.LogTrace("Creating response for CIBA request");

        //////////////////////////
        // access token
        /////////////////////////
        var (accessToken, refreshToken) = await CreateAccessTokenAsync(request.ValidatedRequest);

        var response = new TokenResponse
        {
            AccessToken = accessToken,
            AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
            Custom = request.CustomResponse,
            Scope = request.ValidatedRequest.ValidatedResources.RawScopeValues.ToSpaceSeparatedString()
        };

        //////////////////////////
        // refresh token
        /////////////////////////
        if (refreshToken.IsPresent())
        {
            response.RefreshToken = refreshToken;
        }

        //////////////////////////
        // id token
        /////////////////////////

        // load the client that belongs to the device code
        Client? client = null;

        if (null != request.ValidatedRequest.BackChannelAuthenticationRequest.ClientId)
        {
            // todo: do we need this check?
            client = await Clients.FindEnabledClientByIdAsync(request.ValidatedRequest.BackChannelAuthenticationRequest.ClientId);
        }

        if (null == client)
        {
            throw new InvalidOperationException("Client does not exist anymore.");
        }

        var tokenRequest = new TokenCreationRequest
        {
            Subject = request.ValidatedRequest.BackChannelAuthenticationRequest.Subject,
            AccessTokenToHash = response.AccessToken,
            ValidatedResources = request.ValidatedRequest.ValidatedResources,
            ValidatedRequest = request.ValidatedRequest
        };

        var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest);
        var jwt = await TokenService.CreateSecurityTokenAsync(idToken);
        
        response.IdentityToken = jwt;

        return response;
    }

    /// <summary>
    /// Creates the response for an extension grant request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected virtual Task<TokenResponse> ProcessExtensionGrantRequestAsync(TokenRequestValidationResult request)
    {
        Logger.LogTrace("Creating response for extension grant request");

        return ProcessTokenRequestAsync(request);
    }

    /// <summary>
    /// Creates the response for a token request.
    /// </summary>
    /// <param name="validationResult">The validation result.</param>
    /// <returns></returns>
    protected virtual async Task<TokenResponse> ProcessTokenRequestAsync(TokenRequestValidationResult validationResult)
    {
        (var accessToken, var refreshToken) = await CreateAccessTokenAsync(validationResult.ValidatedRequest);
        var response = new TokenResponse
        {
            AccessToken = accessToken,
            AccessTokenLifetime = validationResult.ValidatedRequest.AccessTokenLifetime,
            Custom = validationResult.CustomResponse,
            Scope = validationResult.ValidatedRequest.ValidatedResources.RawScopeValues.ToSpaceSeparatedString()
        };

        if (refreshToken.IsPresent())
        {
            response.RefreshToken = refreshToken;
        }

        return response;
    }

    /// <summary>
    /// Creates the access/refresh token.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">Client does not exist anymore.</exception>
    protected virtual async Task<(string accessToken, string refreshToken)> CreateAccessTokenAsync(ValidatedTokenRequest request)
    {
        var tokenRequest = new TokenCreationRequest
        {
            Subject = request.Subject,
            ValidatedResources = request.ValidatedResources,
            ValidatedRequest = request
        };

        var createRefreshToken = request.ValidatedResources.Resources.OfflineAccess;
        var authorizedScopes = Enumerable.Empty<string>();
        IEnumerable<string>? authorizedResourceIndicators = null;

        if (null != request.AuthorizationCode)
        {
            // load the client that belongs to the authorization code
            Client? client = null;

            if (null != request.AuthorizationCode.ClientId)
            {
                // todo: do we need this check?
                client = await Clients.FindEnabledClientByIdAsync(request.AuthorizationCode.ClientId);
            }

            if (null == client)
            {
                throw new InvalidOperationException("Client does not exist anymore.");
            }

            tokenRequest.Subject = request.AuthorizationCode.Subject;
            tokenRequest.Description = request.AuthorizationCode.Description;

            authorizedScopes = request.AuthorizationCode.RequestedScopes;
            authorizedResourceIndicators = request.AuthorizationCode.RequestedResourceIndicators;
        }
        else if (null != request.BackChannelAuthenticationRequest)
        {
            // load the client that belongs to the authorization code
            Client? client = null;

            if (null != request.BackChannelAuthenticationRequest.ClientId)
            {
                // todo: do we need this check?
                client = await Clients.FindEnabledClientByIdAsync(request.BackChannelAuthenticationRequest.ClientId);
            }

            if (null == client)
            {
                throw new InvalidOperationException("Client does not exist anymore.");
            }

            tokenRequest.Subject = request.BackChannelAuthenticationRequest.Subject;
            tokenRequest.Description = request.BackChannelAuthenticationRequest.Description;

            authorizedScopes = request.BackChannelAuthenticationRequest.AuthorizedScopes;
            // TODO: should this come from the current request instead of the ciba request
            authorizedResourceIndicators = request.BackChannelAuthenticationRequest.RequestedResourceIndicators;
        }
        else if (null != request.DeviceCode)
        {
            Client? client = null;

            if (null != request.DeviceCode.ClientId)
            {
                // todo: do we need this check?
                client = await Clients.FindEnabledClientByIdAsync(request.DeviceCode.ClientId);
            }

            if (null == client)
            {
                throw new InvalidOperationException("Client does not exist anymore.");
            }

            tokenRequest.Subject = request.DeviceCode.Subject;
            tokenRequest.Description = request.DeviceCode.Description;

            authorizedScopes = request.DeviceCode.AuthorizedScopes;
        }
        else
        {
            authorizedScopes = request.ValidatedResources.RawScopeValues;
        }

        var at = await TokenService.CreateAccessTokenAsync(tokenRequest);
        var accessToken = await TokenService.CreateSecurityTokenAsync(at);

        if (createRefreshToken)
        {
            var rtRequest = new RefreshTokenCreationRequest
            {
                Client = request.Client,
                Subject = tokenRequest.Subject,
                Description = tokenRequest.Description,
                AuthorizedScopes = authorizedScopes,
                AuthorizedResourceIndicators = authorizedResourceIndicators,
                AccessToken = at,
                RequestedResourceIndicator = request.RequestedResourceIndicator,
            };

            var refreshToken = await RefreshTokenService.CreateRefreshTokenAsync(rtRequest);

            return (accessToken, refreshToken);
        }

        return (accessToken, null);
    }

    /// <summary>
    /// Creates an id_token for a refresh token request if identity resources have been requested.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="newAccessToken">The new access token.</param>
    /// <returns></returns>
    protected virtual async Task<string?> CreateIdTokenFromRefreshTokenRequestAsync(ValidatedTokenRequest request, string newAccessToken)
    {
        if (request.RefreshToken.AuthorizedScopes.Contains(OidcConstants.StandardScopes.OpenId))
        {
            var tokenRequest = new TokenCreationRequest
            {
                Subject = request.RefreshToken.Subject,
                ValidatedResources = request.ValidatedResources,
                ValidatedRequest = request,
                AccessTokenToHash = newAccessToken
            };

            var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest);
            return await TokenService.CreateSecurityTokenAsync(idToken);
        }

        return null;
    }
}