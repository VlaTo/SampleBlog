using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Default token service
/// </summary>
public class DefaultTokenService : ITokenService
{
    /// <summary>
    /// The HTTP context accessor
    /// </summary>
    protected IHttpContextAccessor ContextAccessor
    {
        get;
        init;
    }

    /// <summary>
    /// The claims provider
    /// </summary>
    protected IClaimsService ClaimsProvider
    {
        get;
        init;
    }

    /// <summary>
    /// The reference token store
    /// </summary>
    protected IReferenceTokenStore ReferenceTokenStore
    {
        get;
        init;
    }

    /// <summary>
    /// The signing service
    /// </summary>
    protected readonly ITokenCreationService CreationService;

    /// <summary>
    /// The clock
    /// </summary>
    protected ISystemClock Clock
    {
        get;
        init;
    }

    /// <summary>
    /// The key material service
    /// </summary>
    protected IKeyMaterialService KeyMaterialService
    {
        get;
        init;
    }

    /// <summary>
    /// The logger
    /// </summary>
    protected ILogger Logger
    {
        get;
        init;
    }

    /// <summary>
    /// The IdentityServer options
    /// </summary>
    protected readonly IdentityServerOptions Options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTokenService" /> class.
    /// </summary>
    /// <param name="claimsProvider">The claims provider.</param>
    /// <param name="referenceTokenStore">The reference token store.</param>
    /// <param name="creationService">The signing service.</param>
    /// <param name="contextAccessor">The HTTP context accessor.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="keyMaterialService"></param>
    /// <param name="options">The IdentityServer options</param>
    /// <param name="logger">The logger.</param>
    public DefaultTokenService(
        IClaimsService claimsProvider,
        IReferenceTokenStore referenceTokenStore,
        ITokenCreationService creationService,
        IHttpContextAccessor contextAccessor,
        ISystemClock clock,
        IKeyMaterialService keyMaterialService,
        IdentityServerOptions options,
        ILogger<DefaultTokenService> logger)
    {
        ContextAccessor = contextAccessor;
        ClaimsProvider = claimsProvider;
        ReferenceTokenStore = referenceTokenStore;
        CreationService = creationService;
        Clock = clock;
        KeyMaterialService = keyMaterialService;
        Options = options;
        Logger = logger;
    }
}