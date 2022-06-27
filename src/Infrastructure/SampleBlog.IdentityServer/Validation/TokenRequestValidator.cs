using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Validation;

public class TokenRequestValidator : ITokenRequestValidator
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
    private readonly IBackchannelAuthenticationRequestIdValidator backchannelAuthenticationRequestIdValidator;
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
        IBackchannelAuthenticationRequestIdValidator backchannelAuthenticationRequestIdValidator,
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
        this.backchannelAuthenticationRequestIdValidator = backchannelAuthenticationRequestIdValidator;
        this.extensionGrantValidator = extensionGrantValidator;
        this.customRequestValidator = customRequestValidator;
        this.resourceValidator = resourceValidator;
        this.resourceStore = resourceStore;
        this.refreshTokenService = refreshTokenService;
        this.events = events;
        this.clock = clock;
        this.logger = logger;
    }
}