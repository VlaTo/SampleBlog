using System.Collections.Specialized;
using System.Text;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.Contexts;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Core.Events;
using SampleBlog.IdentityServer.Core.Extensions;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Requests;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

internal class TokenRequestValidator : ITokenRequestValidator
{
    private readonly IdentityServerOptions options;
    private readonly IIssuerNameService issuerNameService;
    private readonly IAuthorizationCodeStore authorizationCodeStore;
    private readonly ExtensionGrantValidator extensionGrantValidator;
    private readonly ICustomTokenRequestValidator customRequestValidator;
    private readonly IResourceValidator resourceValidator;
    private readonly IResourceStore resourceStore;
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IEventService events;
    private readonly IResourceOwnerPasswordValidator resourceOwnerValidator;
    private readonly IProfileService profile;
    private readonly IDeviceCodeValidator deviceCodeValidator;
    private readonly IBackChannelAuthenticationRequestIdValidator backChannelAuthenticationRequestIdValidator;
    private readonly ISystemClock clock;
    private readonly ILogger logger;

    private ValidatedTokenRequest validatedRequest;

    public TokenRequestValidator(
        IdentityServerOptions options,
        IIssuerNameService issuerNameService,
        IAuthorizationCodeStore authorizationCodeStore,
        IResourceOwnerPasswordValidator resourceOwnerValidator,
        IProfileService profile,
        IDeviceCodeValidator deviceCodeValidator,
        IBackChannelAuthenticationRequestIdValidator backChannelAuthenticationRequestIdValidator,
        ExtensionGrantValidator extensionGrantValidator,
        ICustomTokenRequestValidator customRequestValidator,
        IResourceValidator resourceValidator,
        IResourceStore resourceStore,
        IRefreshTokenService refreshTokenService,
        IEventService events,
        ISystemClock clock,
        ILogger<TokenRequestValidator> logger)
    {
        this.options = options;
        this.issuerNameService = issuerNameService;
        this.authorizationCodeStore = authorizationCodeStore;
        this.resourceOwnerValidator = resourceOwnerValidator;
        this.profile = profile;
        this.deviceCodeValidator = deviceCodeValidator;
        this.backChannelAuthenticationRequestIdValidator = backChannelAuthenticationRequestIdValidator;
        this.extensionGrantValidator = extensionGrantValidator;
        this.customRequestValidator = customRequestValidator;
        this.resourceValidator = resourceValidator;
        this.resourceStore = resourceStore;
        this.refreshTokenService = refreshTokenService;
        this.events = events;
        this.clock = clock;
        this.logger = logger;
    }

    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="clientValidationResult">The client validation result.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException">
    /// parameters
    /// or
    /// client
    /// </exception>
    public async Task<TokenRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("TokenRequestValidator.ValidateRequest");

        logger.LogDebug("Start token request validation");

        validatedRequest = new ValidatedTokenRequest
        {
            IssuerName = await issuerNameService.GetCurrentAsync(),
            Raw = parameters ?? throw new ArgumentNullException(nameof(parameters)),
            Options = options
        };

        if (clientValidationResult == null) throw new ArgumentNullException(nameof(clientValidationResult));

        validatedRequest.SetClient(clientValidationResult.Client!, clientValidationResult.Secret, clientValidationResult.Confirmation);

        /////////////////////////////////////////////
        // check client protocol type
        /////////////////////////////////////////////
        if (IdentityServerConstants.ProtocolTypes.OpenIdConnect != validatedRequest.Client?.ProtocolType)
        {
            LogError("Invalid protocol type for client",
                new
                {
                    clientId = validatedRequest.Client.ClientId,
                    expectedProtocolType = IdentityServerConstants.ProtocolTypes.OpenIdConnect,
                    actualProtocolType = validatedRequest.Client.ProtocolType
                });

            return Invalid(OidcConstants.TokenErrors.InvalidClient);
        }

        /////////////////////////////////////////////
        // check grant type
        /////////////////////////////////////////////
        var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);

        if (String.IsNullOrEmpty(grantType))
        {
            LogError("Grant type is missing");
            return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        if (options.InputLengthRestrictions.GrantType < grantType.Length)
        {
            LogError("Grant type is too long");
            return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        validatedRequest.GrantType = grantType;

        //////////////////////////////////////////////////////////
        // check for resource indicator and basic formatting
        //////////////////////////////////////////////////////////
        var resourceIndicators = parameters.GetValues(OidcConstants.TokenRequest.Resource) ?? Array.Empty<string>();

        if (resourceIndicators.Any(x => x.Length > options.InputLengthRestrictions.ResourceIndicatorMaxLength))
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Resource indicator maximum length exceeded");
        }

        if (false == resourceIndicators.AreValidResourceIndicatorFormat(logger))
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator format");
        }

        if (1 < resourceIndicators.Length)
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Multiple resource indicators not supported on token endpoint.");
        }

        validatedRequest.RequestedResourceIndicator = resourceIndicators.SingleOrDefault();


        //////////////////////////////////////////////////////////
        // run specific logic for grants
        //////////////////////////////////////////////////////////

        switch (grantType)
        {
            case OidcConstants.GrantTypes.AuthorizationCode:
            {
                return await RunValidationAsync(ValidateAuthorizationCodeRequestAsync, parameters);
            }

            case OidcConstants.GrantTypes.ClientCredentials:
            {
                return await RunValidationAsync(ValidateClientCredentialsRequestAsync, parameters);
            }

            case OidcConstants.GrantTypes.Password:
            {
                return await RunValidationAsync(ValidateResourceOwnerCredentialRequestAsync, parameters);
            }

            case OidcConstants.GrantTypes.RefreshToken:
            {
                return await RunValidationAsync(ValidateRefreshTokenRequestAsync, parameters);
            }

            case OidcConstants.GrantTypes.DeviceCode:
            {
                return await RunValidationAsync(ValidateDeviceCodeRequestAsync, parameters);
            }

            case OidcConstants.GrantTypes.Ciba:
            {
                return await RunValidationAsync(ValidateCibaRequestRequestAsync, parameters);
            }

            default:
            {
                return await RunValidationAsync(ValidateExtensionGrantRequestAsync, parameters);
            }
        }
    }

    private async Task<TokenRequestValidationResult> RunValidationAsync(
        Func<NameValueCollection, Task<TokenRequestValidationResult>> validationFunc,
        NameValueCollection parameters)
    {
        // run standard validation
        var result = await validationFunc(parameters);

        if (result.IsError)
        {
            return result;
        }

        // run custom validation
        logger.LogTrace("Calling into custom request validator: {type}", customRequestValidator.GetType().FullName);

        var customValidationContext = new CustomTokenRequestValidationContext
        {
            Result = result
        };

        await customRequestValidator.ValidateAsync(customValidationContext);

        if (customValidationContext.Result.IsError)
        {
            if (customValidationContext.Result.Error.IsPresent())
            {
                LogError("Custom token request validator", new { error = customValidationContext.Result.Error });
            }
            else
            {
                LogError("Custom token request validator error");
            }

            return customValidationContext.Result;
        }

        LogSuccess();

        //LicenseValidator.ValidateClient(customValidationContext.Result.ValidatedRequest.ClientId);

        return customValidationContext.Result;
    }

    private async Task<TokenRequestValidationResult> ValidateAuthorizationCodeRequestAsync(NameValueCollection parameters)
    {
        logger.LogDebug("Start validation of authorization code token request");

        /////////////////////////////////////////////
        // check if client is authorized for grant type
        /////////////////////////////////////////////
        if (!validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.AuthorizationCode) &&
            !validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.Hybrid))
        {
            LogError("Client not authorized for code flow");
            return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        /////////////////////////////////////////////
        // validate authorization code
        /////////////////////////////////////////////
        var code = parameters.Get(OidcConstants.TokenRequest.Code);
        if (String.IsNullOrEmpty(code))
        {
            LogError("Authorization code is missing");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        if (options.InputLengthRestrictions.AuthorizationCode < code.Length)
        {
            LogError("Authorization code is too long");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        validatedRequest.AuthorizationCodeHandle = code;

        var authorizationCode = await authorizationCodeStore.GetAuthorizationCodeAsync(code);

        if (null == authorizationCode)
        {
            LogError("Invalid authorization code", new { code });
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        /////////////////////////////////////////////
        // validate client binding
        /////////////////////////////////////////////
        if (authorizationCode.ClientId != validatedRequest.Client.ClientId)
        {
            LogError("Client is trying to use a code from a different client", new
            {
                clientId = validatedRequest.Client.ClientId,
                codeClient = authorizationCode.ClientId
            });
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        // remove code from store
        // todo: set to consumed in the future?
        await authorizationCodeStore.RemoveAuthorizationCodeAsync(code);

        if (authorizationCode.CreationTime.HasExceeded(authorizationCode.Lifetime, clock.UtcNow.UtcDateTime))
        {
            LogError("Authorization code expired", new { code });
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        /////////////////////////////////////////////
        // populate session id
        /////////////////////////////////////////////
        if (authorizationCode.SessionId.IsPresent())
        {
            validatedRequest.SessionId = authorizationCode.SessionId;
        }

        /////////////////////////////////////////////
        // validate code expiration
        /////////////////////////////////////////////
        if (authorizationCode.CreationTime.HasExceeded(validatedRequest.Client.AuthorizationCodeLifetime, clock.UtcNow.UtcDateTime))
        {
            LogError("Authorization code is expired");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        validatedRequest.AuthorizationCode = authorizationCode;
        validatedRequest.Subject = authorizationCode.Subject;

        /////////////////////////////////////////////
        // validate redirect_uri
        /////////////////////////////////////////////
        var redirectUri = parameters.Get(OidcConstants.TokenRequest.RedirectUri);

        if (String.IsNullOrEmpty(redirectUri))
        {
            LogError("Redirect URI is missing");
            return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        if (false == redirectUri.Equals(validatedRequest.AuthorizationCode.RedirectUri, StringComparison.Ordinal))
        {
            LogError("Invalid redirect_uri", new
            {
                redirectUri,
                expectedRedirectUri = validatedRequest.AuthorizationCode.RedirectUri
            });
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        /////////////////////////////////////////////
        // validate scopes are present
        /////////////////////////////////////////////
        if (validatedRequest.AuthorizationCode.RequestedScopes == null ||
            !validatedRequest.AuthorizationCode.RequestedScopes.Any())
        {
            LogError("Authorization code has no associated scopes");
            return Invalid(OidcConstants.TokenErrors.InvalidRequest);
        }

        //////////////////////////////////////////////////////////
        // resource indicator
        //////////////////////////////////////////////////////////
        if (validatedRequest.RequestedResourceIndicator != null &&
            validatedRequest.AuthorizationCode.RequestedResourceIndicators?.Any() == true &&
            !validatedRequest.AuthorizationCode.RequestedResourceIndicators.Contains(validatedRequest.RequestedResourceIndicator))
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Resource indicator does not match any resource indicator in the original authorize request.");
        }

        //////////////////////////////////////////////////////////
        // resource and scope validation 
        //////////////////////////////////////////////////////////
        var validatedResources = await resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest
        {
            Client = validatedRequest.Client,
            Scopes = validatedRequest.AuthorizationCode.RequestedScopes,
            ResourceIndicators = validatedRequest.AuthorizationCode.RequestedResourceIndicators,
            // if we are issuing a refresh token, then we need to allow the non-isolated resource
            IncludeNonIsolatedApiResources = validatedRequest.AuthorizationCode.RequestedScopes.Contains(OidcConstants.StandardScopes.OfflineAccess)
        });

        if (false == validatedResources.Succeeded)
        {
            if (validatedResources.InvalidResourceIndicators.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator.");
            }
            if (validatedResources.InvalidScopes.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidScope, "Invalid scope.");
            }
        }

        //LicenseValidator.ValidateResourceIndicators(_validatedRequest.RequestedResourceIndicator);
        validatedRequest.ValidatedResources = validatedResources.FilterByResourceIndicator(validatedRequest.RequestedResourceIndicator);

        /////////////////////////////////////////////
        // validate PKCE parameters
        /////////////////////////////////////////////
        var codeVerifier = parameters.Get(OidcConstants.TokenRequest.CodeVerifier);

        if (validatedRequest.Client.RequirePkce || validatedRequest.AuthorizationCode.CodeChallenge.IsPresent())
        {
            logger.LogDebug("Client required a proof key for code exchange. Starting PKCE validation");

            var proofKeyResult = ValidateAuthorizationCodeWithProofKeyParameters(codeVerifier, validatedRequest.AuthorizationCode);

            if (proofKeyResult.IsError)
            {
                return proofKeyResult;
            }

            validatedRequest.CodeVerifier = codeVerifier;
        }
        else
        {
            if (codeVerifier.IsPresent())
            {
                LogError("Unexpected code_verifier: {codeVerifier}. This happens when the client is trying to use PKCE, but it is not enabled. Set RequirePkce to true.", codeVerifier);
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }
        }

        /////////////////////////////////////////////
        // make sure user is enabled
        /////////////////////////////////////////////
        var context = new IsActiveContext(validatedRequest.AuthorizationCode.Subject, validatedRequest.Client, IdentityServerConstants.ProfileIsActiveCallers.AuthorizationCodeValidation);

        await profile.IsActiveAsync(context);

        if (false == context.IsActive)
        {
            LogError("User has been disabled", new
            {
                subjectId = validatedRequest.AuthorizationCode.Subject.GetSubjectId()
            });
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        logger.LogDebug("Validation of authorization code token request success");

        return Valid();
    }
    
    private TokenRequestValidationResult ValidateAuthorizationCodeWithProofKeyParameters(string codeVerifier, AuthorizationCode authorizationCode)
    {
        if (authorizationCode.CodeChallenge.IsMissing() || authorizationCode.CodeChallengeMethod.IsMissing())
        {
            LogError("Client is missing code challenge or code challenge method", new
            {
                clientId = validatedRequest.Client.ClientId
            });
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        if (codeVerifier.IsMissing())
        {
            LogError("Missing code_verifier");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        if (codeVerifier.Length < options.InputLengthRestrictions.CodeVerifierMinLength ||
            codeVerifier.Length > options.InputLengthRestrictions.CodeVerifierMaxLength)
        {
            LogError("code_verifier is too short or too long");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        if (false == Constants.SupportedCodeChallengeMethods.Contains(authorizationCode.CodeChallengeMethod))
        {
            LogError("Unsupported code challenge method", new { codeChallengeMethod = authorizationCode.CodeChallengeMethod });
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        if (false == ValidateCodeVerifierAgainstCodeChallenge(codeVerifier, authorizationCode.CodeChallenge, authorizationCode.CodeChallengeMethod))
        {
            LogError("Transformed code verifier does not match code challenge");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        return Valid();
    }

    private async Task<TokenRequestValidationResult> ValidateClientCredentialsRequestAsync(NameValueCollection parameters)
    {
        logger.LogDebug("Start client credentials token request validation");

        /////////////////////////////////////////////
        // check if client is authorized for grant type
        /////////////////////////////////////////////
        if (!validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.ClientCredentials))
        {
            LogError("Client not authorized for client credentials flow, check the AllowedGrantTypes setting", new
            {
                clientId = validatedRequest.Client.ClientId
            });
            return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        /////////////////////////////////////////////
        // check if client is allowed to request scopes
        /////////////////////////////////////////////
        var scopeError = await ValidateRequestedScopesAndResourcesAsync(parameters, ignoreImplicitIdentityScopes: true, ignoreImplicitOfflineAccess: true);

        if (null != scopeError)
        {
            return Invalid(scopeError);
        }

        if (validatedRequest.ValidatedResources.Resources.IdentityResources.Any())
        {
            LogError("Client cannot request OpenID scopes in client credentials flow", new
            {
                clientId = validatedRequest.Client.ClientId
            });
            return Invalid(OidcConstants.TokenErrors.InvalidScope);
        }

        if (validatedRequest.ValidatedResources.Resources.OfflineAccess)
        {
            LogError("Client cannot request a refresh token in client credentials flow", new
            {
                clientId = validatedRequest.Client.ClientId
            });
            return Invalid(OidcConstants.TokenErrors.InvalidScope);
        }

        logger.LogDebug("{clientId} credentials token request validation success", validatedRequest.Client.ClientId);

        return Valid();
    }

    // todo: do we want to rework the semantics of these ignore params?
    // also seems like other workflows other than CC clients can omit scopes?
    private async Task<string?> ValidateRequestedScopesAndResourcesAsync(
        NameValueCollection parameters,
        bool ignoreImplicitIdentityScopes = false,
        bool ignoreImplicitOfflineAccess = false)
    {
        var scopes = parameters.Get(OidcConstants.TokenRequest.Scope);

        if (scopes.IsMissing())
        {
            logger.LogTrace("Client provided no scopes - checking allowed scopes list");

            if (!validatedRequest.Client.AllowedScopes.IsNullOrEmpty())
            {
                // this finds all the scopes the client is allowed to access
                var clientAllowedScopes = new List<string>();

                if (false == ignoreImplicitIdentityScopes)
                {
                    var resources = await resourceStore.FindResourcesByScopeAsync(validatedRequest.Client.AllowedScopes);
                    clientAllowedScopes.AddRange(resources.ToScopeNames().Where(x => validatedRequest.Client.AllowedScopes.Contains(x)));
                }
                else
                {
                    var apiScopes = await resourceStore.FindApiScopesByNameAsync(validatedRequest.Client.AllowedScopes);
                    clientAllowedScopes.AddRange(apiScopes.Select(x => x.Name));
                }

                if (false == ignoreImplicitOfflineAccess)
                {
                    if (validatedRequest.Client.AllowOfflineAccess)
                    {
                        clientAllowedScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                    }
                }

                scopes = clientAllowedScopes.Distinct().ToSpaceSeparatedString();
                
                logger.LogTrace("Defaulting to: {scopes}", scopes);
            }
            else
            {
                LogError("No allowed scopes configured for client", new
                {
                    clientId = validatedRequest.Client.ClientId
                });
                return OidcConstants.TokenErrors.InvalidScope;
            }
        }

        if (options.InputLengthRestrictions.Scope < scopes.Length)
        {
            LogError("Scope parameter exceeds max allowed length");
            return OidcConstants.TokenErrors.InvalidScope;
        }

        var requestedScopes = scopes.ParseScopesString();

        if (null == requestedScopes)
        {
            LogError("No scopes found in request");
            return OidcConstants.TokenErrors.InvalidScope;
        }

        var resourceIndicators = validatedRequest.RequestedResourceIndicator != null
            ? new[] { validatedRequest.RequestedResourceIndicator }
            : Enumerable.Empty<string>();

        var resourceValidationResult = await resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest
        {
            Client = validatedRequest.Client,
            Scopes = requestedScopes,
            ResourceIndicators = resourceIndicators,
            // if the client is passing explicit scopes, we want to exclude the non-isolated resource scenaio from validation
            IncludeNonIsolatedApiResources = parameters.Get(OidcConstants.TokenRequest.Scope).IsMissing()
        });

        if (false == resourceValidationResult.Succeeded)
        {
            if (resourceValidationResult.InvalidResourceIndicators.Any())
            {
                LogError("Invalid resource indicator");
                return OidcConstants.TokenErrors.InvalidTarget;
            }

            if (resourceValidationResult.InvalidScopes.Any())
            {
                LogError("Invalid scopes requested");
            }
            else
            {
                LogError("Invalid scopes for client requested");
            }

            return OidcConstants.TokenErrors.InvalidScope;
        }

        validatedRequest.RequestedScopes = requestedScopes;

        //LicenseValidator.ValidateResourceIndicators(_validatedRequest.RequestedResourceIndicator);
        validatedRequest.ValidatedResources = resourceValidationResult.FilterByResourceIndicator(validatedRequest.RequestedResourceIndicator);

        return null;
    }

    private async Task<TokenRequestValidationResult> ValidateExtensionGrantRequestAsync(NameValueCollection parameters)
    {
        logger.LogDebug("Start validation of custom grant token request");

        /////////////////////////////////////////////
        // check if client is allowed to use grant type
        /////////////////////////////////////////////
        if (false == validatedRequest.Client.AllowedGrantTypes.Contains(validatedRequest.GrantType))
        {
            LogError("Client does not have the custom grant type in the allowed list, therefore requested grant is not allowed", new
            {
                clientId = validatedRequest.Client.ClientId
            });
            return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        /////////////////////////////////////////////
        // check if a validator is registered for the grant type
        /////////////////////////////////////////////
        var grantTypes = extensionGrantValidator.GetAvailableGrantTypes();

        if (false == grantTypes.Contains(validatedRequest.GrantType, StringComparer.Ordinal))
        {
            LogError("No validator is registered for the grant type", new
            {
                grantType = validatedRequest.GrantType
            });
            return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        /////////////////////////////////////////////
        // check if client is allowed to request scopes
        /////////////////////////////////////////////
        var scopeError = await ValidateRequestedScopesAndResourcesAsync(parameters);

        if (null != scopeError)
        {
            return Invalid(scopeError);
        }

        /////////////////////////////////////////////
        // validate custom grant type
        /////////////////////////////////////////////
        var result = await extensionGrantValidator.ValidateAsync(validatedRequest);

        if (null == result)
        {
            LogError("Invalid extension grant");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        if (result.IsError)
        {
            if (result.Error.IsPresent())
            {
                LogError("Invalid extension grant", new { error = result.Error });
                return Invalid(result.Error, result.ErrorDescription, result.CustomResponse);
            }

            LogError("Invalid extension grant");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant, customResponse: result.CustomResponse);
        }

        if (null != result.Subject)
        {
            /////////////////////////////////////////////
            // make sure user is enabled
            /////////////////////////////////////////////
            var context = new IsActiveContext(
                result.Subject,
                validatedRequest.Client,
                IdentityServerConstants.ProfileIsActiveCallers.ExtensionGrantValidation
            );

            await profile.IsActiveAsync(context);

            if (false == context.IsActive)
            {
                // todo: raise event?

                LogError("User has been disabled", new
                {
                    subjectId = result.Subject.GetSubjectId()
                });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            validatedRequest.Subject = result.Subject;
        }

        logger.LogDebug("Validation of extension grant token request success");

        return Valid(result.CustomResponse);
    }

    private async Task<TokenRequestValidationResult> ValidateDeviceCodeRequestAsync(NameValueCollection parameters)
    {
        logger.LogDebug("Start validation of device code request");

        /////////////////////////////////////////////
        // resource indicator not supported for device flow
        /////////////////////////////////////////////
        if (null != validatedRequest.RequestedResourceIndicator)
        {
            LogError("Resource indicators not supported for device flow");
            return Invalid(OidcConstants.TokenErrors.InvalidTarget);
        }

        /////////////////////////////////////////////
        // check if client is authorized for grant type
        /////////////////////////////////////////////
        var grantTypes = validatedRequest.Client.AllowedGrantTypes.ToList();

        if (false == grantTypes.Contains(GrantType.DeviceFlow))
        {
            LogError("Client not authorized for device flow");
            return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        /////////////////////////////////////////////
        // validate device code parameter
        /////////////////////////////////////////////
        var deviceCode = parameters.Get(OidcConstants.TokenRequest.DeviceCode);

        if (String.IsNullOrEmpty(deviceCode))
        {
            LogError("Device code is missing");
            return Invalid(OidcConstants.TokenErrors.InvalidRequest);
        }

        if (options.InputLengthRestrictions.DeviceCode < deviceCode.Length)
        {
            LogError("Device code too long");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        /////////////////////////////////////////////
        // validate device code
        /////////////////////////////////////////////
        var deviceCodeContext = new DeviceCodeValidationContext
        {
            DeviceCode = deviceCode,
            Request = validatedRequest
        };

        await deviceCodeValidator.ValidateAsync(deviceCodeContext);

        if (deviceCodeContext.Result.IsError)
        {
            return deviceCodeContext.Result;
        }

        //////////////////////////////////////////////////////////
        // scope validation 
        //////////////////////////////////////////////////////////
        var validatedResources = await resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest
        {
            Client = validatedRequest.Client,
            Scopes = validatedRequest.DeviceCode.AuthorizedScopes,
            ResourceIndicators = null // not supported for device grant
        });

        if (false == validatedResources.Succeeded)
        {
            if (validatedResources.InvalidResourceIndicators.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator.");
            }

            if (validatedResources.InvalidScopes.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidScope, "Invalid scope.");
            }
        }

        //LicenseValidator.ValidateResourceIndicators(_validatedRequest.RequestedResourceIndicator);
        validatedRequest.ValidatedResources = validatedResources;

        logger.LogDebug("Validation of device code token request success");

        return Valid();
    }

    private async Task<TokenRequestValidationResult> ValidateRefreshTokenRequestAsync(NameValueCollection parameters)
    {
        logger.LogDebug("Start validation of refresh token request");

        var refreshTokenHandle = parameters.Get(OidcConstants.TokenRequest.RefreshToken);

        if (String.IsNullOrEmpty(refreshTokenHandle))
        {
            LogError("Refresh token is missing");
            return Invalid(OidcConstants.TokenErrors.InvalidRequest);
        }

        if (options.InputLengthRestrictions.RefreshToken < refreshTokenHandle.Length)
        {
            LogError("Refresh token too long");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        var result = await refreshTokenService.ValidateRefreshTokenAsync(refreshTokenHandle, validatedRequest.Client);

        if (result.IsError)
        {
            LogWarning("Refresh token validation failed. aborting");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        validatedRequest.RefreshToken = result.RefreshToken;
        validatedRequest.RefreshTokenHandle = refreshTokenHandle;
        validatedRequest.Subject = result.RefreshToken.Subject;

        //////////////////////////////////////////////////////////
        // resource indicator
        //////////////////////////////////////////////////////////
        var resourceIndicators = validatedRequest.RefreshToken.AuthorizedResourceIndicators;

        if (null != validatedRequest.RefreshToken.AuthorizedResourceIndicators)
        {
            // we had an authorization request so check current requested resource against original list
            if (validatedRequest.RequestedResourceIndicator != null &&
                !validatedRequest.RefreshToken.AuthorizedResourceIndicators.Contains(validatedRequest.RequestedResourceIndicator))
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Resource indicator does not match any resource indicator in the original authorize request.");
            }
        }
        else if (false == String.IsNullOrWhiteSpace(validatedRequest.RequestedResourceIndicator))
        {
            resourceIndicators = new[] { validatedRequest.RequestedResourceIndicator };
        }

        //////////////////////////////////////////////////////////
        // resource and scope validation 
        //////////////////////////////////////////////////////////
        var validatedResources = await resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest
        {
            Client = validatedRequest.Client,
            Scopes = validatedRequest.RefreshToken.AuthorizedScopes,
            ResourceIndicators = resourceIndicators,
            // we're issuing refresh token, so we need to allow for non-isolated resource
            IncludeNonIsolatedApiResources = true,
        });

        if (false == validatedResources.Succeeded)
        {
            if (validatedResources.InvalidResourceIndicators.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator.");
            }

            if (validatedResources.InvalidScopes.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidScope, "Invalid scope.");
            }
        }

        //LicenseValidator.ValidateResourceIndicators(_validatedRequest.RequestedResourceIndicator);
        validatedRequest.ValidatedResources = validatedResources.FilterByResourceIndicator(validatedRequest.RequestedResourceIndicator);

        logger.LogDebug("Validation of refresh token request success");
        // todo: more logging - similar to TokenValidator before

        return Valid();
    }

    private async Task<TokenRequestValidationResult> ValidateCibaRequestRequestAsync(NameValueCollection parameters)
    {
        logger.LogDebug("Start validation of CIBA request");

        /////////////////////////////////////////////
        // check if client is authorized for grant type
        /////////////////////////////////////////////
        var grantTypes = validatedRequest.Client.AllowedGrantTypes.ToList();

        if (false == grantTypes.Contains(OidcConstants.GrantTypes.Ciba))
        {
            LogError("Client not authorized for CIBA flow");
            return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        //LicenseValidator.ValidateCiba();

        /////////////////////////////////////////////
        // validate authentication request id parameter
        /////////////////////////////////////////////
        var authRequestId = parameters.Get(OidcConstants.TokenRequest.AuthenticationRequestId);

        if (authRequestId.IsMissing())
        {
            LogError("Authentication request id is missing");
            return Invalid(OidcConstants.TokenErrors.InvalidRequest);
        }

        if (options.InputLengthRestrictions.AuthenticationRequestId < authRequestId.Length)
        {
            LogError("Authentication request id too long");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        /////////////////////////////////////////////
        // validate authentication request id
        /////////////////////////////////////////////
        var validationContext = new BackchannelAuthenticationRequestIdValidationContext
        {
            AuthenticationRequestId = authRequestId,
            Request = validatedRequest
        };

        await backChannelAuthenticationRequestIdValidator.ValidateAsync(validationContext);

        if (validationContext.Result.IsError)
        {
            return validationContext.Result;
        }

        //////////////////////////////////////////////////////////
        // resource indicator
        //////////////////////////////////////////////////////////
        if (validatedRequest.RequestedResourceIndicator != null &&
            validatedRequest.BackChannelAuthenticationRequest.RequestedResourceIndicators.Any() &&
            false == validatedRequest.BackChannelAuthenticationRequest.RequestedResourceIndicators.Contains(validatedRequest.RequestedResourceIndicator))
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Resource indicator does not match any resource indicator in the original backchannel authentication request.");
        }

        //////////////////////////////////////////////////////////
        // resource and scope validation 
        //////////////////////////////////////////////////////////
        var validatedResources = await resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest
        {
            Client = validatedRequest.Client,
            Scopes = validatedRequest.BackChannelAuthenticationRequest.AuthorizedScopes,
            ResourceIndicators = validatedRequest.BackChannelAuthenticationRequest.RequestedResourceIndicators,
            // if we are issuing a refresh token, then we need to allow the non-isolated resource
            IncludeNonIsolatedApiResources = validatedRequest.BackChannelAuthenticationRequest.RequestedScopes.Contains(OidcConstants.StandardScopes.OfflineAccess)
        });

        if (false == validatedResources.Succeeded)
        {
            if (validatedResources.InvalidResourceIndicators.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator.");
            }

            if (validatedResources.InvalidScopes.Any())
            {
                return Invalid(OidcConstants.AuthorizeErrors.InvalidScope, "Invalid scope.");
            }
        }

        //LicenseValidator.ValidateResourceIndicators(_validatedRequest.RequestedResourceIndicator);
        validatedRequest.ValidatedResources = validatedResources.FilterByResourceIndicator(validatedRequest.RequestedResourceIndicator);
        
        logger.LogDebug("Validation of CIBA token request success");

        return Valid();
    }

    private async Task<TokenRequestValidationResult> ValidateResourceOwnerCredentialRequestAsync(NameValueCollection parameters)
    {
        logger.LogDebug("Start resource owner password token request validation");

        /////////////////////////////////////////////
        // check if client is authorized for grant type
        /////////////////////////////////////////////
        if (false == validatedRequest.Client.AllowedGrantTypes.Contains(GrantType.ResourceOwnerPassword))
        {
            LogError("Client not authorized for resource owner flow, check the AllowedGrantTypes setting", new
            {
                client_id = validatedRequest.Client.ClientId
            });
            return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
        }

        /////////////////////////////////////////////
        // check if client is allowed to request scopes
        /////////////////////////////////////////////
        var scopeError = await ValidateRequestedScopesAndResourcesAsync(parameters);

        if (null != scopeError)
        {
            return Invalid(scopeError);
        }

        /////////////////////////////////////////////
        // check resource owner credentials
        /////////////////////////////////////////////
        var userName = parameters.Get(OidcConstants.TokenRequest.UserName);
        var password = parameters.Get(OidcConstants.TokenRequest.Password);

        if (String.IsNullOrEmpty(userName))
        {
            LogError("Username is missing");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        if (String.IsNullOrEmpty(password))
        {
            password = String.Empty;
        }

        if (userName.Length > options.InputLengthRestrictions.UserName ||
            password.Length > options.InputLengthRestrictions.Password)
        {
            LogError("Username or password too long");
            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        validatedRequest.UserName = userName;
        
        /////////////////////////////////////////////
        // authenticate user
        /////////////////////////////////////////////
        var resourceOwnerContext = new ResourceOwnerPasswordValidationContext(userName, password, validatedRequest);

        await resourceOwnerValidator.ValidateAsync(resourceOwnerContext);

        if (resourceOwnerContext.Result.IsError)
        {
            // protect against bad validator implementations
            resourceOwnerContext.Result.Error ??= OidcConstants.TokenErrors.InvalidGrant;

            if (OidcConstants.TokenErrors.UnsupportedGrantType == resourceOwnerContext.Result.Error)
            {
                LogError("Resource owner password credential grant type not supported");
                await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, "password grant type not supported", resourceOwnerContext.Request.Client.ClientId);

                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType, customResponse: resourceOwnerContext.Result.CustomResponse);
            }

            var errorDescription = "invalid_username_or_password";

            if (resourceOwnerContext.Result.ErrorDescription.IsPresent())
            {
                errorDescription = resourceOwnerContext.Result.ErrorDescription;
            }

            LogInformation("User authentication failed: ", errorDescription ?? resourceOwnerContext.Result.Error);
            await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, errorDescription, resourceOwnerContext.Request.Client.ClientId);

            return Invalid(resourceOwnerContext.Result.Error, errorDescription, resourceOwnerContext.Result.CustomResponse);
        }

        if (null == resourceOwnerContext.Result.Subject)
        {
            const string error = "User authentication failed: no principal returned";
            LogError(error);
            await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, error, resourceOwnerContext.Request.Client.ClientId);

            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        /////////////////////////////////////////////
        // make sure user is enabled
        /////////////////////////////////////////////
        var context = new IsActiveContext(
            resourceOwnerContext.Result.Subject,
            validatedRequest.Client,
            IdentityServerConstants.ProfileIsActiveCallers.ResourceOwnerValidation
        );

        await profile.IsActiveAsync(context);

        if (false == context.IsActive)
        {
            LogError("User has been disabled", new { subjectId = resourceOwnerContext.Result.Subject.GetSubjectId() });
            await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, "user is inactive", resourceOwnerContext.Request.Client.ClientId);

            return Invalid(OidcConstants.TokenErrors.InvalidGrant);
        }

        validatedRequest.UserName = userName;
        validatedRequest.Subject = resourceOwnerContext.Result.Subject;

        await RaiseSuccessfulResourceOwnerAuthenticationEventAsync(
            userName,
            resourceOwnerContext.Result.Subject.GetSubjectId(),
            resourceOwnerContext.Request.Client.ClientId
        );

        logger.LogDebug("Resource owner password token request validation success.");

        return Valid(resourceOwnerContext.Result.CustomResponse);
    }

    private static bool ValidateCodeVerifierAgainstCodeChallenge(string codeVerifier, string codeChallenge, string codeChallengeMethod)
    {
        if (codeChallengeMethod == OidcConstants.CodeChallengeMethods.Plain)
        {
            return TimeConstantComparer.IsEqual(codeVerifier.Sha256(), codeChallenge);
        }

        var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
        var hashedBytes = codeVerifierBytes.Sha256();
        var transformedCodeVerifier = Base64Url.Encode(hashedBytes);

        return TimeConstantComparer.IsEqual(transformedCodeVerifier.Sha256(), codeChallenge);
    }

    private TokenRequestValidationResult Valid(Dictionary<string, object>? customResponse = null)
    {
        return new TokenRequestValidationResult(validatedRequest, customResponse);
    }

    private TokenRequestValidationResult Invalid(string error, string? errorDescription = null, Dictionary<string, object>? customResponse = null)
    {
        return new TokenRequestValidationResult(validatedRequest, error, errorDescription, customResponse);
    }

    private void LogError(string? message = null, object? values = null)
    {
        LogWithRequestDetails(LogLevel.Error, message, values);
    }

    private void LogWarning(string? message = null, object? values = null)
    {
        LogWithRequestDetails(LogLevel.Warning, message, values);
    }

    private void LogInformation(string? message = null, object? values = null)
    {
        LogWithRequestDetails(LogLevel.Information, message, values);
    }

    private void LogWithRequestDetails(LogLevel logLevel, string? message = null, object? values = null)
    {
        var details = new TokenRequestValidationLog(validatedRequest, options.Logging.TokenRequestSensitiveValuesFilter);

        if (false == String.IsNullOrEmpty(message))
        {
            try
            {
                if (null == values)
                {
                    logger.Log(logLevel, message + ", {@details}", details);
                }
                else
                {
                    logger.Log(logLevel, message + "{@values}, details: {@details}", values, details);
                }

            }
            catch (Exception ex)
            {
                logger.LogError("Error logging {exception}, request details: {@details}", ex.Message, details);
            }
        }
        else
        {
            logger.Log(logLevel, "{@details}", details);
        }
    }

    private void LogSuccess()
    {
        LogWithRequestDetails(LogLevel.Information, "Token request validation success");
    }

    private Task RaiseSuccessfulResourceOwnerAuthenticationEventAsync(string userName, string subjectId, string clientId)
    {
        return events.RaiseAsync(new UserLoginSuccessEvent(userName, subjectId, null, interactive: false, clientId));
    }

    private Task RaiseFailedResourceOwnerAuthenticationEventAsync(string userName, string error, string clientId)
    {
        return events.RaiseAsync(new UserLoginFailureEvent(userName, error, interactive: false, clientId: clientId));
    }
}