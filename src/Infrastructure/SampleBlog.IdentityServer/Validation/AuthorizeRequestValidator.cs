using System.Collections.Specialized;
using System.Security.Claims;
using IdentityModel;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Validation;

internal sealed class AuthorizeRequestValidator : IAuthorizeRequestValidator
{
    private readonly ResponseTypeEqualityComparer responseTypeComparer;

    private readonly IdentityServerOptions options;
    private readonly IIssuerNameService issuerNameService;
    private readonly IClientStore clients;
    private readonly ICustomAuthorizeRequestValidator customValidator;
    private readonly IRedirectUriValidator uriValidator;
    private readonly IResourceValidator resourceValidator;
    private readonly IUserSession userSession;
    private readonly IJwtRequestValidator jwtRequestValidator;
    private readonly IJwtRequestUriHttpClient jwtRequestUriHttpClient;
    private readonly ILogger logger;
    
    public AuthorizeRequestValidator(
        IdentityServerOptions options,
        IIssuerNameService issuerNameService,
        IClientStore clients,
        ICustomAuthorizeRequestValidator customValidator,
        IRedirectUriValidator uriValidator,
        IResourceValidator resourceValidator,
        IUserSession userSession,
        IJwtRequestValidator jwtRequestValidator,
        IJwtRequestUriHttpClient jwtRequestUriHttpClient,
        ILogger<AuthorizeRequestValidator> logger)
    {
        responseTypeComparer = new ResponseTypeEqualityComparer();

        this.options = options;
        this.issuerNameService = issuerNameService;
        this.clients = clients;
        this.customValidator = customValidator;
        this.uriValidator = uriValidator;
        this.resourceValidator = resourceValidator;
        this.jwtRequestValidator = jwtRequestValidator;
        this.userSession = userSession;
        this.jwtRequestUriHttpClient = jwtRequestUriHttpClient;
        this.logger = logger;
    }

    public async Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal? subject = null)
    {
        //using var activity = Tracing.BasicActivitySource.StartActivity("AuthorizeRequestValidator.Validate");

        logger.LogDebug("Start authorize request protocol validation");

        var request = new ValidatedAuthorizeRequest
        {
            Options = options,
            IssuerName = await issuerNameService.GetCurrentAsync(),
            Subject = subject ?? Principal.Anonymous,
            Raw = parameters ?? throw new ArgumentNullException(nameof(parameters))
        };

        // load client_id
        // client_id must always be present on the request
        var loadClientResult = await LoadClientAsync(request);

        if (loadClientResult.IsError)
        {
            return loadClientResult;
        }

        // load request object
        var roLoadResult = await LoadRequestObjectAsync(request);

        if (roLoadResult.IsError)
        {
            return roLoadResult;
        }

        // validate request object
        var roValidationResult = await ValidateRequestObjectAsync(request);

        if (roValidationResult.IsError)
        {
            return roValidationResult;
        }

        // validate client_id and redirect_uri
        var clientResult = await ValidateClientAsync(request);

        if (clientResult.IsError)
        {
            return clientResult;
        }

        // state, response_type, response_mode
        var mandatoryResult = ValidateCoreParameters(request);

        if (mandatoryResult.IsError)
        {
            return mandatoryResult;
        }

        // scope, scope restrictions and plausability, and resource indicators
        var scopeResult = await ValidateScopeAndResourceAsync(request);

        if (scopeResult.IsError)
        {
            return scopeResult;
        }

        // nonce, prompt, acr_values, login_hint etc.
        var optionalResult = await ValidateOptionalParametersAsync(request);

        if (optionalResult.IsError)
        {
            return optionalResult;
        }

        // custom validator
        //logger.LogDebug("Calling into custom validator: {type}", customValidator.GetType().FullName);

        var context = new CustomAuthorizeRequestValidationContext
        {
            Result = new AuthorizeRequestValidationResult(request)
        };

        await customValidator.ValidateAsync(context);

        var customResult = context.Result;
        if (customResult.IsError)
        {
            LogError("Error in custom validation", customResult.Error, request);
            return Invalid(request, customResult.Error, customResult.ErrorDescription);
        }

        logger.LogTrace("Authorize request protocol validation successful");

        //LicenseValidator.ValidateClient(request.ClientId);

        return Valid(request);
    }

    private async Task<AuthorizeRequestValidationResult> LoadRequestObjectAsync(ValidatedAuthorizeRequest request)
    {
        var jwtRequest = request.Raw?.Get(OidcConstants.AuthorizeRequest.Request);
        var jwtRequestUri = request.Raw?.Get(OidcConstants.AuthorizeRequest.RequestUri);

        if (jwtRequest.IsPresent() && jwtRequestUri.IsPresent())
        {
            LogError("Both request and request_uri are present", request);
            return Invalid(request, description: "Only one request parameter is allowed");
        }

        if (options.Endpoints.EnableJwtRequestUri)
        {
            if (jwtRequestUri.IsPresent())
            {
                // 512 is from the spec
                if (512 < jwtRequestUri.Length)
                {
                    LogError("request_uri is too long", request);
                    return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestUri, description: "request_uri is too long");
                }

                var jwt = await jwtRequestUriHttpClient.GetJwtAsync(jwtRequestUri, request.Client);

                if (jwt.IsMissing())
                {
                    LogError("no value returned from request_uri", request);
                    return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestUri, description: "no value returned from request_uri");
                }

                jwtRequest = jwt;
            }
        }
        else if (jwtRequestUri.IsPresent())
        {
            LogError("request_uri present but config prohibits", request);
            return Invalid(request, error: OidcConstants.AuthorizeErrors.RequestUriNotSupported);
        }

        // check length restrictions
        if (jwtRequest.IsPresent())
        {
            if (jwtRequest.Length >= options.InputLengthRestrictions.Jwt)
            {
                LogError("request value is too long", request);
                return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestObject, description: "Invalid request value");
            }
        }

        request.RequestObject = jwtRequest;

        return Valid(request);
    }

    private async Task<AuthorizeRequestValidationResult> LoadClientAsync(ValidatedAuthorizeRequest request)
    {
        var clientId = request.Raw?.Get(OidcConstants.AuthorizeRequest.ClientId);

        if (String.IsNullOrEmpty(clientId) || clientId.Length > options.InputLengthRestrictions.ClientId)
        {
            LogError("client_id is missing or too long", request);
            return Invalid(request, description: "Invalid client_id");
        }

        request.ClientId = clientId;

        var client = await clients.FindEnabledClientByIdAsync(request.ClientId);

        if (null == client)
        {
            LogError("Unknown client or not enabled", request.ClientId, request);
            return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, "Unknown client or client not enabled");
        }

        request.SetClient(client);

        return Valid(request);
    }

    private async Task<AuthorizeRequestValidationResult> ValidateRequestObjectAsync(ValidatedAuthorizeRequest request)
    {
        //////////////////////////////////////////////////////////
        // validate request object
        /////////////////////////////////////////////////////////
        if (request.RequestObject.IsPresent())
        {
            // validate the request JWT for this client
            var jwtRequestValidationResult = await jwtRequestValidator.ValidateAsync(new JwtRequestValidationContext
            {
                Client = request.Client,
                JwtTokenString = request.RequestObject
            });

            if (jwtRequestValidationResult.IsError)
            {
                LogError("request JWT validation failure", request);
                return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestObject, description: "Invalid JWT request");
            }

            // validate response_type match
            var responseType = request.Raw?.Get(OidcConstants.AuthorizeRequest.ResponseType);

            if (null != responseType)
            {
                var payloadResponseType = jwtRequestValidationResult.Payload
                    .SingleOrDefault(c => c.Type == OidcConstants.AuthorizeRequest.ResponseType)?.Value;

                if (false == String.IsNullOrEmpty(payloadResponseType))
                {
                    if (payloadResponseType != responseType)
                    {
                        LogError("response_type in JWT payload does not match response_type in request", request);
                        return Invalid(request, description: "Invalid JWT request");
                    }
                }
            }

            // validate client_id mismatch
            var payloadClientId = jwtRequestValidationResult.Payload
                .SingleOrDefault(c => c.Type == OidcConstants.AuthorizeRequest.ClientId)?.Value;

            if (false == String.IsNullOrEmpty(payloadClientId))
            {
                if (false == String.Equals(request.Client?.ClientId, payloadClientId, StringComparison.Ordinal))
                {
                    LogError("client_id in JWT payload does not match client_id in request", request);
                    return Invalid(request, description: "Invalid JWT request");
                }
            }
            else
            {
                LogError("client_id is missing in JWT payload", request);
                return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestObject, description: "Invalid JWT request");

            }

            var ignoreKeys = new[]
            {
                JwtClaimTypes.Issuer,
                JwtClaimTypes.Audience
            };

            // merge jwt payload values into original request parameters
            // 1. clear the keys in the raw collection for every key found in the request object
            foreach (var claimType in jwtRequestValidationResult.Payload.Select(c => c.Type).Distinct())
            {
                var qsValue = request.Raw?.Get(claimType);

                if (null != qsValue)
                {
                    request.Raw.Remove(claimType);
                }
            }

            // 2. copy over the value
            foreach (var claim in jwtRequestValidationResult.Payload)
            {
                request.Raw.Add(claim.Type, claim.Value);
            }

            var ruri = request.Raw?.Get(OidcConstants.AuthorizeRequest.RequestUri);

            if (null != ruri)
            {
                request.Raw.Remove(OidcConstants.AuthorizeRequest.RequestUri);
                request.Raw.Add(OidcConstants.AuthorizeRequest.Request, request.RequestObject);
            }
            
            request.RequestObjectValues = jwtRequestValidationResult.Payload;
        }

        return Valid(request);
    }

    private async Task<AuthorizeRequestValidationResult> ValidateClientAsync(ValidatedAuthorizeRequest request)
    {
        //////////////////////////////////////////////////////////
        // check request object requirement
        //////////////////////////////////////////////////////////
        if (request.Client.RequireRequestObject)
        {
            if (!request.RequestObjectValues.Any())
            {
                return Invalid(request, description: "Client must use request object, but no request or request_uri parameter present");
            }
        }

        //////////////////////////////////////////////////////////
        // redirect_uri must be present, and a valid uri
        //////////////////////////////////////////////////////////
        var redirectUri = request.Raw.Get(OidcConstants.AuthorizeRequest.RedirectUri);

        if (null != redirectUri && redirectUri.Length >= options.InputLengthRestrictions.RedirectUri)
        {
            LogError("redirect_uri is missing or too long", request);
            return Invalid(request, description: "Invalid redirect_uri");
        }

        if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
        {
            LogError("malformed redirect_uri", redirectUri, request);
            return Invalid(request, description: "Invalid redirect_uri");
        }

        //////////////////////////////////////////////////////////
        // check if client protocol type is oidc
        //////////////////////////////////////////////////////////
        if (request.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.OpenIdConnect)
        {
            LogError("Invalid protocol type for OIDC authorize endpoint", request.Client.ProtocolType, request);
            return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, description: "Invalid protocol");
        }

        //////////////////////////////////////////////////////////
        // check if redirect_uri is valid
        //////////////////////////////////////////////////////////
        if (await uriValidator.IsRedirectUriValidAsync(redirectUri, request.Client) == false)
        {
            LogError("Invalid redirect_uri", redirectUri, request);
            return Invalid(request, OidcConstants.AuthorizeErrors.InvalidRequest, "Invalid redirect_uri");
        }

        request.RedirectUri = redirectUri;

        return Valid(request);
    }

    private AuthorizeRequestValidationResult ValidateCoreParameters(ValidatedAuthorizeRequest request)
    {
        //////////////////////////////////////////////////////////
        // check state
        //////////////////////////////////////////////////////////
        var state = request.Raw?.Get(OidcConstants.AuthorizeRequest.State);

        if (null != state)
        {
            request.State = state;
        }

        //////////////////////////////////////////////////////////
        // response_type must be present and supported
        //////////////////////////////////////////////////////////
        var responseType = request.Raw?.Get(OidcConstants.AuthorizeRequest.ResponseType);

        if (null != responseType)
        {
            LogError("Missing response_type", request);
            return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, "Missing response_type");
        }

        // The responseType may come in in an unconventional order.
        // Use an IEqualityComparer that doesn't care about the order of multiple values.
        // Per https://tools.ietf.org/html/rfc6749#section-3.1.1 -
        // 'Extension response types MAY contain a space-delimited (%x20) list of
        // values, where the order of values does not matter (e.g., response
        // type "a b" is the same as "b a").'
        // http://openid.net/specs/oauth-v2-multiple-response-types-1_0-03.html#terminology -
        // 'If a response type contains one of more space characters (%20), it is compared
        // as a space-delimited list of values in which the order of values does not matter.'
        if (false == Constants.SupportedResponseTypes.Contains(responseType, responseTypeComparer))
        {
            LogError("Response type not supported", responseType, request);
            return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, "Response type not supported");
        }

        // Even though the responseType may have come in in an unconventional order,
        // we still need the request's ResponseType property to be set to the
        // conventional, supported response type.
        request.ResponseType = Constants.SupportedResponseTypes.First(
            supportedResponseType => responseTypeComparer.Equals(supportedResponseType, responseType)
        );

        request.GrantType = Constants.ResponseTypeToGrantTypeMapping[request.ResponseType];
        request.ResponseMode = Constants.AllowedResponseModesForGrantType[request.GrantType].First();

        //////////////////////////////////////////////////////////
        // check if flow is allowed at authorize endpoint
        //////////////////////////////////////////////////////////
        if (!Constants.AllowedGrantTypesForAuthorizeEndpoint.Contains(request.GrantType))
        {
            LogError("Invalid grant type", request.GrantType, request);
            return Invalid(request, description: "Invalid response_type");
        }

        //////////////////////////////////////////////////////////
        // check if PKCE is required and validate parameters
        //////////////////////////////////////////////////////////
        if (request.GrantType is GrantType.AuthorizationCode or GrantType.Hybrid)
        {
            logger.LogDebug("Checking for PKCE parameters");

            /////////////////////////////////////////////////////////////////////////////
            // validate code_challenge and code_challenge_method
            /////////////////////////////////////////////////////////////////////////////
            var proofKeyResult = ValidatePkceParameters(request);

            if (proofKeyResult.IsError)
            {
                return proofKeyResult;
            }
        }

        //////////////////////////////////////////////////////////
        // check response_mode parameter and set response_mode
        //////////////////////////////////////////////////////////

        // check if response_mode parameter is present and valid
        var responseMode = request.Raw.Get(OidcConstants.AuthorizeRequest.ResponseMode);
        if (responseMode.IsPresent())
        {
            if (Constants.SupportedResponseModes.Contains(responseMode))
            {
                if (Constants.AllowedResponseModesForGrantType[request.GrantType].Contains(responseMode))
                {
                    request.ResponseMode = responseMode;
                }
                else
                {
                    LogError("Invalid response_mode for response_type", responseMode, request);
                    return Invalid(request, OidcConstants.AuthorizeErrors.InvalidRequest, description: "Invalid response_mode for response_type");
                }
            }
            else
            {
                LogError("Unsupported response_mode", responseMode, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, description: "Invalid response_mode");
            }
        }


        //////////////////////////////////////////////////////////
        // check if grant type is allowed for client
        //////////////////////////////////////////////////////////
        if (!request.Client.AllowedGrantTypes.Contains(request.GrantType))
        {
            LogError("Invalid grant type for client", request.GrantType, request);
            return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, "Invalid grant type for client");
        }

        //////////////////////////////////////////////////////////
        // check if response type contains an access token,
        // and if client is allowed to request access token via browser
        //////////////////////////////////////////////////////////
        var responseTypes = responseType.FromSpaceSeparatedString();

        if (responseTypes.Contains(OidcConstants.ResponseTypes.Token))
        {
            if (false == request.Client.AllowAccessTokensViaBrowser)
            {
                LogError("Client requested access token - but client is not configured to receive access tokens via browser", request);
                return Invalid(request, description: "Client not configured to receive access tokens via browser");
            }
        }

        return Valid(request);
    }

    private AuthorizeRequestValidationResult ValidatePkceParameters(ValidatedAuthorizeRequest request)
    {
        var fail = Invalid(request);
        var codeChallenge = request.Raw?.Get(OidcConstants.AuthorizeRequest.CodeChallenge);

        if (null != codeChallenge)
        {
            if (request.Client is { RequirePkce: true })
            {
                LogError("code_challenge is missing", request);
                fail.ErrorDescription = "code challenge required";
            }
            else
            {
                logger.LogDebug("No PKCE used.");
                return Valid(request);
            }

            return fail;
        }

        if (codeChallenge.Length < options.InputLengthRestrictions.CodeChallengeMinLength ||
            codeChallenge.Length > options.InputLengthRestrictions.CodeChallengeMaxLength)
        {
            LogError("code_challenge is either too short or too long", request);
            fail.ErrorDescription = "Invalid code_challenge";
            return fail;
        }

        request.CodeChallenge = codeChallenge;

        var codeChallengeMethod = request.Raw?.Get(OidcConstants.AuthorizeRequest.CodeChallengeMethod);

        if (null != codeChallengeMethod)
        {
            logger.LogDebug("Missing code_challenge_method, defaulting to plain");
            codeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain;
        }

        if (false == Constants.SupportedCodeChallengeMethods.Contains(codeChallengeMethod))
        {
            LogError("Unsupported code_challenge_method", codeChallengeMethod, request);
            fail.ErrorDescription = "Transform algorithm not supported";
            return fail;
        }

        // check if plain method is allowed
        if (OidcConstants.CodeChallengeMethods.Plain == codeChallengeMethod)
        {
            if (false == request.Client.AllowPlainTextPkce)
            {
                LogError("code_challenge_method of plain is not allowed", request);
                fail.ErrorDescription = "Transform algorithm not supported";
                return fail;
            }
        }

        request.CodeChallengeMethod = codeChallengeMethod;

        return Valid(request);
    }

    private async Task<AuthorizeRequestValidationResult> ValidateScopeAndResourceAsync(ValidatedAuthorizeRequest request)
    {
        var scope = request.Raw?.Get(OidcConstants.AuthorizeRequest.Scope);

        if (null == scope)
        {
            LogError("scope is missing", request);
            return Invalid(request, description: "Invalid scope");
        }

        if (scope.Length > options.InputLengthRestrictions.Scope)
        {
            LogError("scopes too long.", request);
            return Invalid(request, description: "Invalid scope");
        }

        request.RequestedScopes = scope.FromSpaceSeparatedString().Distinct().ToList();
        request.IsOpenIdRequest = request.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OpenId);
        
        var requirement = Constants.ResponseTypeToScopeRequirement[request.ResponseType];

        if (requirement is Constants.ScopeRequirement.Identity or Constants.ScopeRequirement.IdentityOnly)
        {
            if (false == request.IsOpenIdRequest)
            {
                LogError("response_type requires the openid scope", request);
                return Invalid(request, description: "Missing openid scope");
            }
        }
        
        var resourceIndicators = request.Raw?.GetValues(OidcConstants.TokenRequest.Resource) ?? Array.Empty<string>();

        if (resourceIndicators.Any(x => x.Length > options.InputLengthRestrictions.ResourceIndicatorMaxLength))
        {
            return Invalid(request, OidcConstants.AuthorizeErrors.InvalidTarget, "Resource indicator maximum length exceeded");
        }

        if (false == resourceIndicators.AreValidResourceIndicatorFormat(logger))
        {
            return Invalid(request, OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator format");
        }

        // we don't want to allow resource indicators when "token" is requested to authorize endpoint
        if (GrantType.Implicit == request.GrantType && resourceIndicators.Any())
        {
            // todo: correct error?
            return Invalid(request, OidcConstants.AuthorizeErrors.InvalidTarget, "Resource indicators not allowed for response_type 'token'.");
        }

        request.RequestedResourceIndiators = resourceIndicators;

        var validationRequest = new ResourceValidationRequest
        {
            Client = request.Client,
            Scopes = request.RequestedScopes,
            ResourceIndicators = resourceIndicators,
            IncludeNonIsolatedApiResources = request.RequestedScopes.Contains(OidcConstants.StandardScopes.OfflineAccess),
        };

        var validatedResources = await resourceValidator.ValidateRequestedResourcesAsync(validationRequest);

        if (false == validatedResources.Succeeded)
        {
            if (validatedResources.InvalidResourceIndicators.Any())
            {
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator");
            }

            if (validatedResources.InvalidScopes.Any())
            {
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope, "Invalid scope");
            }
        }

        //LicenseValidator.ValidateResourceIndicators(resourceIndicators);

        if (validatedResources.Resources.IdentityResources.Any() && false == request.IsOpenIdRequest)
        {
            LogError("Identity related scope requests, but no openid scope", request);
            return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope, "Identity scopes requested, but openid scope is missing");
        }

        if (validatedResources.Resources.ApiScopes.Any())
        {
            request.IsApiResourceRequest = true;
        }

        var responseTypeValidationCheck = true;

        switch (requirement)
        {
            case Constants.ScopeRequirement.Identity:
            {
                if (false == validatedResources.Resources.IdentityResources.Any())
                {
                    logger.LogError("Requests for id_token response type must include identity scopes");
                    responseTypeValidationCheck = false;
                }

                break;
            }

            case Constants.ScopeRequirement.IdentityOnly:
            {
                if (false == validatedResources.Resources.IdentityResources.Any() || validatedResources.Resources.ApiScopes.Any())
                {
                    logger.LogError("Requests for id_token response type only must not include resource scopes");
                    responseTypeValidationCheck = false;
                }

                break;
            }

            case Constants.ScopeRequirement.ResourceOnly:
            {
                if (validatedResources.Resources.IdentityResources.Any() || false == validatedResources.Resources.ApiScopes.Any())
                {
                    logger.LogError("Requests for token response type only must include resource scopes, but no identity scopes.");
                    responseTypeValidationCheck = false;
                }

                break;
            }
        }

        if (false == responseTypeValidationCheck)
        {
            return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope, "Invalid scope for response type");
        }

        request.ValidatedResources = validatedResources;

        return Valid(request);
    }

    private async Task<AuthorizeRequestValidationResult> ValidateOptionalParametersAsync(ValidatedAuthorizeRequest request)
    {
        var nonce = request.Raw?.Get(OidcConstants.AuthorizeRequest.Nonce);

        if (nonce.IsPresent())
        {
            if (nonce.Length > options.InputLengthRestrictions.Nonce)
            {
                LogError("Nonce too long", request);
                return Invalid(request, description: "Invalid nonce");
            }

            request.Nonce = nonce;
        }
        else
        {
            if (request.ResponseType.FromSpaceSeparatedString().Contains(IdentityServerConstants.TokenTypes.IdentityToken))
            {
                LogError("Nonce required for flow with id_token response type", request);
                return Invalid(request, description: "Invalid nonce");
            }
        }

        var prompt = request.Raw?.Get(OidcConstants.AuthorizeRequest.Prompt);

        if (prompt.IsPresent())
        {
            var prompts = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (prompts.All(p => Constants.SupportedPromptModes.Contains(p)))
            {
                if (prompts.Contains(OidcConstants.PromptModes.None) && prompts.Length > 1)
                {
                    LogError("prompt contains 'none' and other values. 'none' should be used by itself.", request);
                    return Invalid(request, description: "Invalid prompt");
                }

                request.OriginalPromptModes = prompts;
            }
            else
            {
                logger.LogDebug("Unsupported prompt mode - ignored: " + prompt);
            }
        }

        var suppressed_prompt = request.Raw?.Get(Constants.SuppressedPrompt);

        if (suppressed_prompt.IsPresent())
        {
            var prompts = suppressed_prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (prompts.All(p => Constants.SupportedPromptModes.Contains(p)))
            {
                if (prompts.Contains(OidcConstants.PromptModes.None) && prompts.Length > 1)
                {
                    LogError("suppressed_prompt contains 'none' and other values. 'none' should be used by itself.", request);
                    return Invalid(request, description: "Invalid prompt");
                }

                request.SuppressedPromptModes = prompts;
            }
            else
            {
                logger.LogDebug("Unsupported suppressed_prompt mode - ignored: " + prompt);
            }
        }

        request.PromptModes = request.OriginalPromptModes.Except(request.SuppressedPromptModes).ToArray();

        var uilocales = request.Raw?.Get(OidcConstants.AuthorizeRequest.UiLocales);

        if (uilocales.IsPresent())
        {
            if (uilocales.Length > options.InputLengthRestrictions.UiLocale)
            {
                LogError("UI locale too long", request);
                return Invalid(request, description: "Invalid ui_locales");
            }

            request.UiLocales = uilocales;
        }

        var display = request.Raw?.Get(OidcConstants.AuthorizeRequest.Display);

        if (display.IsPresent())
        {
            if (Constants.SupportedDisplayModes.Contains(display))
            {
                request.DisplayMode = display;
            }

            logger.LogDebug("Unsupported display mode - ignored: " + display);
        }

        var maxAge = request.Raw?.Get(OidcConstants.AuthorizeRequest.MaxAge);

        if (maxAge.IsPresent())
        {
            if (int.TryParse(maxAge, out var seconds))
            {
                if (seconds >= 0)
                {
                    request.MaxAge = seconds;
                }
                else
                {
                    LogError("Invalid max_age.", request);
                    return Invalid(request, description: "Invalid max_age");
                }
            }
            else
            {
                LogError("Invalid max_age.", request);
                return Invalid(request, description: "Invalid max_age");
            }
        }

        var loginHint = request.Raw?.Get(OidcConstants.AuthorizeRequest.LoginHint);

        if (loginHint.IsPresent())
        {
            if (options.InputLengthRestrictions.LoginHint < loginHint.Length)
            {
                LogError("Login hint too long", request);
                return Invalid(request, description: "Invalid login_hint");
            }

            request.LoginHint = loginHint;
        }

        var acrValues = request.Raw?.Get(OidcConstants.AuthorizeRequest.AcrValues);

        if (acrValues.IsPresent())
        {
            if (options.InputLengthRestrictions.AcrValues < acrValues.Length)
            {
                LogError("Acr values too long", request);
                return Invalid(request, description: "Invalid acr_values");
            }

            request.AuthenticationContextReferenceClasses = acrValues.FromSpaceSeparatedString().Distinct().ToList();
        }

        var idp = request.GetIdP();

        if (idp.IsPresent())
        {
            // if idp is present but client does not allow it, strip it from the request message
            if (null != request.Client?.IdentityProviderRestrictions && request.Client.IdentityProviderRestrictions.Any())
            {
                if (false == request.Client.IdentityProviderRestrictions.Contains(idp))
                {
                    logger.LogWarning("idp requested ({idp}) is not in client restriction list.", idp);
                    request.RemoveIdP();
                }
            }
        }

        if (options.Endpoints.EnableCheckSessionEndpoint)
        {
            if (request.Subject.IsAuthenticated())
            {
                var sessionId = await userSession.GetSessionIdAsync();

                if (sessionId.IsPresent())
                {
                    request.SessionId = sessionId;
                }
                else
                {
                    LogError("Check session endpoint enabled, but SessionId is missing", request);
                }
            }
            else
            {
                request.SessionId = String.Empty; // empty string for anonymous users
            }
        }

        return Valid(request);
    }

    private AuthorizeRequestValidationResult Invalid(
        ValidatedAuthorizeRequest request,
        string error = OidcConstants.AuthorizeErrors.InvalidRequest,
        string? description = null)
    {
        return new AuthorizeRequestValidationResult(request, error, description);
    }

    private AuthorizeRequestValidationResult Valid(ValidatedAuthorizeRequest request)
    {
        return new AuthorizeRequestValidationResult(request);
    }

    private void LogError(string message, ValidatedAuthorizeRequest request)
    {
        //var requestDetails = new AuthorizeRequestValidationLog(request, options.Logging.AuthorizeRequestSensitiveValuesFilter);
        //logger.LogError(message + "\n{@requestDetails}", requestDetails);
    }

    private void LogError(string message, string detail, ValidatedAuthorizeRequest request)
    {
        //var requestDetails = new AuthorizeRequestValidationLog(request, options.Logging.AuthorizeRequestSensitiveValuesFilter);
        //logger.LogError(message + ": {detail}\n{@requestDetails}", detail, requestDetails);
    }
}