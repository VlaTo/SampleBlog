using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Core.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;

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

    /// <summary>
    /// Finds device authorization by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <returns></returns>
    public Task<DeviceCode?> FindByDeviceCodeAsync(string deviceCode)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultDeviceFlowCodeService.FindByDeviceCode");

        return store.FindByDeviceCodeAsync(deviceCode.Sha256());
    }

    /// <summary>
    /// Finds device authorization by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <returns></returns>
    public Task<DeviceCode?> FindByUserCodeAsync(string userCode)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultDeviceFlowCodeService.FindByUserCode");

        return store.FindByUserCodeAsync(userCode.Sha256());
    }

    /// <summary>
    /// Updates device authorization, searching by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultDeviceFlowCodeService.UpdateByUserCode");

        return store.UpdateByUserCodeAsync(userCode.Sha256(), data);
    }

    /// <summary>
    /// Removes the device authorization, searching by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <returns></returns>
    public Task RemoveByDeviceCodeAsync(string deviceCode)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultDeviceFlowCodeService.RemoveByDeviceCode");

        return store.RemoveByDeviceCodeAsync(deviceCode.Sha256());
    }
}