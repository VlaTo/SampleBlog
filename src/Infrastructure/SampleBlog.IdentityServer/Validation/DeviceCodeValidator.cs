using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Services;

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

}