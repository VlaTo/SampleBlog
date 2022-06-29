using System.Collections.Specialized;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Stores;
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

    private ValidatedTokenRequest _validatedRequest;

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

        _validatedRequest = new ValidatedTokenRequest
        {
            IssuerName = await _issuerNameService.GetCurrentAsync(),
            Raw = parameters ?? throw new ArgumentNullException(nameof(parameters)),
            Options = _options
        };

        if (clientValidationResult == null) throw new ArgumentNullException(nameof(clientValidationResult));

        _validatedRequest.SetClient(clientValidationResult.Client, clientValidationResult.Secret, clientValidationResult.Confirmation);

        /////////////////////////////////////////////
        // check client protocol type
        /////////////////////////////////////////////
        if (_validatedRequest.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.OpenIdConnect)
        {
            LogError("Invalid protocol type for client",
                new
                {
                    clientId = _validatedRequest.Client.ClientId,
                    expectedProtocolType = IdentityServerConstants.ProtocolTypes.OpenIdConnect,
                    actualProtocolType = _validatedRequest.Client.ProtocolType
                });

            return Invalid(OidcConstants.TokenErrors.InvalidClient);
        }

        /////////////////////////////////////////////
        // check grant type
        /////////////////////////////////////////////
        var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);
        if (grantType.IsMissing())
        {
            LogError("Grant type is missing");
            return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        if (grantType.Length > _options.InputLengthRestrictions.GrantType)
        {
            LogError("Grant type is too long");
            return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
        }

        _validatedRequest.GrantType = grantType;

        //////////////////////////////////////////////////////////
        // check for resource indicator and basic formatting
        //////////////////////////////////////////////////////////
        var resourceIndicators = parameters.GetValues(OidcConstants.TokenRequest.Resource) ?? Enumerable.Empty<string>();

        if (resourceIndicators?.Any(x => x.Length > _options.InputLengthRestrictions.ResourceIndicatorMaxLength) == true)
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Resource indicator maximum length exceeded");
        }

        if (!resourceIndicators.AreValidResourceIndicatorFormat(_logger))
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Invalid resource indicator format");
        }

        if (resourceIndicators.Count() > 1)
        {
            return Invalid(OidcConstants.AuthorizeErrors.InvalidTarget, "Multiple resource indicators not supported on token endpoint.");
        }

        _validatedRequest.RequestedResourceIndicator = resourceIndicators.SingleOrDefault();


        //////////////////////////////////////////////////////////
        // run specific logic for grants
        //////////////////////////////////////////////////////////

        switch (grantType)
        {
            case OidcConstants.GrantTypes.AuthorizationCode:
                return await RunValidationAsync(ValidateAuthorizationCodeRequestAsync, parameters);
            case OidcConstants.GrantTypes.ClientCredentials:
                return await RunValidationAsync(ValidateClientCredentialsRequestAsync, parameters);
            case OidcConstants.GrantTypes.Password:
                return await RunValidationAsync(ValidateResourceOwnerCredentialRequestAsync, parameters);
            case OidcConstants.GrantTypes.RefreshToken:
                return await RunValidationAsync(ValidateRefreshTokenRequestAsync, parameters);
            case OidcConstants.GrantTypes.DeviceCode:
                return await RunValidationAsync(ValidateDeviceCodeRequestAsync, parameters);
            case OidcConstants.GrantTypes.Ciba:
                return await RunValidationAsync(ValidateCibaRequestRequestAsync, parameters);
            default:
                return await RunValidationAsync(ValidateExtensionGrantRequestAsync, parameters);
        }
    }
}