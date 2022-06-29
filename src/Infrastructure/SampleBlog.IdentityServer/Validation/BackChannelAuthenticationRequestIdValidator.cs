using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Contexts;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Default implementation of IBackchannelAuthenticationRequestIdValidator.
/// </summary>
internal class BackChannelAuthenticationRequestIdValidator : IBackChannelAuthenticationRequestIdValidator
{
    private readonly IBackChannelAuthenticationRequestStore backchannelAuthenticationStore;
    private readonly IBackChannelAuthenticationThrottlingService throttlingService;
    private readonly ILogger<BackChannelAuthenticationRequestIdValidator> logger;
    private readonly IProfileService profile;
    private readonly ISystemClock clock;

    public BackChannelAuthenticationRequestIdValidator(
        IBackChannelAuthenticationRequestStore backchannelAuthenticationStore,
        IProfileService profile,
        IBackChannelAuthenticationThrottlingService throttlingService,
        ISystemClock clock,
        ILogger<BackChannelAuthenticationRequestIdValidator> logger)
    {
        this.backchannelAuthenticationStore = backchannelAuthenticationStore;
        this.profile = profile;
        this.throttlingService = throttlingService;
        this.clock = clock;
        this.logger = logger;
    }

    /// <inheritdoc cref="ValidateAsync" />
    public async Task ValidateAsync(BackchannelAuthenticationRequestIdValidationContext context)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("BackchannelAuthenticationRequestIdValidator.Validate");

        var request = await backchannelAuthenticationStore.GetByAuthenticationRequestIdAsync(context.AuthenticationRequestId);

        if (null == request)
        {
            logger.LogError("Invalid authentication request id");
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);

            return;
        }

        // validate client binding
        if (request.ClientId != context.Request.Client?.ClientId)
        {
            logger.LogError("Client {0} is trying to use a authentication request id from client {1}", context.Request.Client.ClientId, request.ClientId);
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);

            return;
        }

        if (await throttlingService.ShouldSlowDown(context.AuthenticationRequestId, request))
        {
            logger.LogError("Client {0} is polling too fast", request.ClientId);
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.SlowDown);

            return;
        }

        // validate lifetime
        if ((request.CreationTime + request.Lifetime) < clock.UtcNow.UtcDateTime)
        {
            logger.LogError("Expired authentication request id");
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.ExpiredToken);

            return;
        }

        // denied
        if (request.IsComplete && (null == request.AuthorizedScopes || request.AuthorizedScopes.Any() == false))
        {
            logger.LogError("No scopes authorized for backchannel authentication request. Access denied");
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.AccessDenied);
            
            await backchannelAuthenticationStore.RemoveByInternalIdAsync(request.InternalId);
            
            return;
        }

        // make sure authentication request id is complete
        if (false == request.IsComplete)
        {
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.AuthorizationPending);
            return;
        }

        // make sure user is enabled
        var isActiveCtx = new IsActiveContext(
            request.Subject,
            context.Request.Client,
            IdentityServerConstants.ProfileIsActiveCallers.BackchannelAuthenticationRequestIdValidation
        );
        await profile.IsActiveAsync(isActiveCtx);

        if (false == isActiveCtx.IsActive)
        {
            logger.LogError("User has been disabled: {subjectId}", request.Subject.GetSubjectId());
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);

            return;
        }

        context.Request.BackChannelAuthenticationRequest = request;
        context.Request.Subject = request.Subject;
        context.Request.SessionId = request.SessionId;

        context.Result = new TokenRequestValidationResult(context.Request);

        await backchannelAuthenticationStore.RemoveByInternalIdAsync(request.InternalId);

        logger.LogDebug("Success validating backchannel authentication request id.");
    }
}