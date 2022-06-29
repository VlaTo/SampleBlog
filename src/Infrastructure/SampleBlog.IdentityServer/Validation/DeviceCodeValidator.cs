using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Contexts;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates an incoming token request using the device flow
/// </summary>
internal class DeviceCodeValidator : IDeviceCodeValidator
{
    private readonly IDeviceFlowCodeService devices;
    private readonly IProfileService profile;
    private readonly IDeviceFlowThrottlingService throttlingService;
    private readonly ISystemClock systemClock;
    private readonly ILogger<DeviceCodeValidator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceCodeValidator"/> class.
    /// </summary>
    /// <param name="devices">The devices.</param>
    /// <param name="profile">The profile.</param>
    /// <param name="throttlingService">The throttling service.</param>
    /// <param name="systemClock">The system clock.</param>
    /// <param name="logger">The logger.</param>
    public DeviceCodeValidator(
        IDeviceFlowCodeService devices,
        IProfileService profile,
        IDeviceFlowThrottlingService throttlingService,
        ISystemClock systemClock,
        ILogger<DeviceCodeValidator> logger)
    {
        this.devices = devices;
        this.profile = profile;
        this.throttlingService = throttlingService;
        this.systemClock = systemClock;
        this.logger = logger;
    }

    /// <summary>
    /// Validates the device code.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task ValidateAsync(DeviceCodeValidationContext context)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity("DeviceCodeValidator.Validate");

        var deviceCode = await devices.FindByDeviceCodeAsync(context.DeviceCode);

        if (null == deviceCode)
        {
            logger.LogError("Invalid device code");
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);

            return;
        }

        // validate client binding
        if (context.Request.Client?.ClientId != deviceCode.ClientId)
        {
            logger.LogError("Client {0} is trying to use a device code from client {1}", context.Request.Client.ClientId, deviceCode.ClientId);
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);

            return;
        }

        if (await throttlingService.ShouldSlowDown(context.DeviceCode, deviceCode))
        {
            logger.LogError("Client {0} is polling too fast", deviceCode.ClientId);
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.SlowDown);

            return;
        }

        // validate lifetime
        if (systemClock.UtcNow.UtcDateTime > deviceCode.CreationTime.AddSeconds(deviceCode.Lifetime))
        {
            logger.LogError("Expired device code");
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.ExpiredToken);

            return;
        }

        // denied
        if (deviceCode.IsAuthorized && (null == deviceCode.AuthorizedScopes || false == deviceCode.AuthorizedScopes.Any()))
        {
            logger.LogError("No scopes authorized for device authorization. Access denied");
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.AccessDenied);

            return;
        }

        // make sure code is authorized
        if (false == deviceCode.IsAuthorized || null == deviceCode.Subject)
        {
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.AuthorizationPending);
            return;
        }

        // make sure user is enabled
        var isActiveCtx = new IsActiveContext(
            deviceCode.Subject,
            context.Request.Client!,
            IdentityServerConstants.ProfileIsActiveCallers.DeviceCodeValidation
        );

        await profile.IsActiveAsync(isActiveCtx);

        if (false == isActiveCtx.IsActive)
        {
            logger.LogError("User has been disabled: {subjectId}", deviceCode.Subject.GetSubjectId());
            context.Result = new TokenRequestValidationResult(context.Request, OidcConstants.TokenErrors.InvalidGrant);

            return;
        }

        context.Request.DeviceCode = deviceCode;
        context.Request.SessionId = deviceCode.SessionId;

        context.Result = new TokenRequestValidationResult(context.Request);

        await devices.RemoveByDeviceCodeAsync(context.DeviceCode);
    }
}