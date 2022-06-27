using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Core.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Stores;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Default wrapper service for IDeviceFlowStore, handling key hashing
/// </summary>
/// <seealso cref="IDeviceFlowCodeService" />
public class DefaultDeviceFlowCodeService : IDeviceFlowCodeService
{
    private readonly IDeviceFlowStore store;
    private readonly IHandleGenerationService handleGenerationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDeviceFlowCodeService"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    public DefaultDeviceFlowCodeService(IDeviceFlowStore store, IHandleGenerationService handleGenerationService)
    {
        this.store = store;
        this.handleGenerationService = handleGenerationService;
    }

    /// <summary>
    /// Stores the device authorization request.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public async Task<string> StoreDeviceAuthorizationAsync(string userCode, DeviceCode data)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultDeviceFlowCodeService.SendLogoutNotifStoreDeviceAuthorization");

        var deviceCode = await handleGenerationService.GenerateAsync();

        await store.StoreDeviceAuthorizationAsync(deviceCode.Sha256(), userCode.Sha256(), data);

        return deviceCode;
    }
}