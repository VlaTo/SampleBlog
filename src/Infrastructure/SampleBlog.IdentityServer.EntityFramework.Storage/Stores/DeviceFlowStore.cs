using IdentityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Services;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Storage.Stores.Serialization;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Stores;

/// <summary>
/// Implementation of IDeviceFlowStore thats uses EF.
/// </summary>
/// <seealso cref="IDeviceFlowStore" />
public class DeviceFlowStore : IDeviceFlowStore
{
    /// <summary>
    /// The DbContext.
    /// </summary>
    protected IPersistedGrantDbContext Context
    {
        get;
    }

    /// <summary>
    ///  The serializer.
    /// </summary>
    protected IPersistentGrantSerializer Serializer
    {
        get;
    }

    /// <summary>
    /// The CancellationToken provider.
    /// </summary>
    protected ICancellationTokenProvider CancellationTokenProvider
    {
        get;
    }

    /// <summary>
    /// The logger.
    /// </summary>
    protected ILogger Logger
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceFlowStore"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="serializer">The serializer</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationTokenProvider"></param>
    public DeviceFlowStore(
        IPersistedGrantDbContext context,
        IPersistentGrantSerializer serializer,
        ILogger<DeviceFlowStore> logger,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        Context = context;
        Serializer = serializer;
        Logger = logger;
        CancellationTokenProvider = cancellationTokenProvider;
    }

    /// <summary>
    /// Stores the device authorization request.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <param name="userCode">The user code.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public virtual async Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DeviceFlowStore.StoreDeviceAuthorization");

        Context.DeviceFlowCodes.Add(ToEntity(data, deviceCode, userCode));

        await Context.SaveChangesAsync(CancellationTokenProvider.CancellationToken);
    }

    /// <summary>
    /// Finds device authorization by user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <returns></returns>
    public virtual async Task<DeviceCode> FindByUserCodeAsync(string userCode)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DeviceFlowStore.FindByUserCode");

        var deviceFlowCode = await Context.DeviceFlowCodes
            .Where(x => x.UserCode == userCode)
            //.ToArrayAsync(CancellationTokenProvider.CancellationToken)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserCode == userCode, CancellationTokenProvider.CancellationToken);

        var model = ToModel(deviceFlowCode?.Data);

        Logger.LogDebug("{userCode} found in database: {userCodeFound}", userCode, model != null);

        return model;
    }

    /// <summary>
    /// Finds device authorization by device code.
    /// </summary>
    /// <param name="deviceCode">The device code.</param>
    /// <returns></returns>
    public virtual async Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DeviceFlowStore.FindByDeviceCode");

        var deviceFlowCodes = (await Context.DeviceFlowCodes.AsNoTracking().Where(x => x.DeviceCode == deviceCode)
                .ToArrayAsync(CancellationTokenProvider.CancellationToken))
            .SingleOrDefault(x => x.DeviceCode == deviceCode);
        var model = ToModel(deviceFlowCodes?.Data);

        Logger.LogDebug("{deviceCode} found in database: {deviceCodeFound}", deviceCode, model != null);

        return model;
    }

    /// <summary>
    /// Converts a model to an entity.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="deviceCode"></param>
    /// <param name="userCode"></param>
    /// <returns></returns>
    protected DeviceFlowCodes ToEntity(DeviceCode model, string deviceCode, string userCode)
    {
        return new DeviceFlowCodes
        {
            DeviceCode = deviceCode,
            UserCode = userCode,
            ClientId = model.ClientId,
            SubjectId = model.Subject.FindFirst(JwtClaimTypes.Subject).Value,
            SessionId = model.SessionId,
            Description = model.Description,
            CreationTime = model.CreationTime,
            Expiration = model.CreationTime.AddSeconds(model.Lifetime),
            Data = Serializer.Serialize(model)
        };
    }

    /// <summary>
    /// Converts a serialized DeviceCode to a model.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected DeviceCode ToModel(string entity)
    {
        if (entity == null) return null;

        return Serializer.Deserialize<DeviceCode>(entity);
    }
}