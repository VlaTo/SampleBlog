using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// CORS policy service that configures the allowed origins from a list of clients' redirect URLs.
/// </summary>
public class InMemoryCorsPolicyService : ICorsPolicyService
{
    /// <summary>
    /// Logger
    /// </summary>
    protected ILogger Logger
    {
        init;
        get;
    }

    /// <summary>
    /// Clients applications list
    /// </summary>
    protected IEnumerable<Client> Clients
    {
        init;
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCorsPolicyService"/> class.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="clients">The clients.</param>
    public InMemoryCorsPolicyService(IEnumerable<Client>? clients, ILogger<InMemoryCorsPolicyService> logger)
    {
        Clients = clients ?? Enumerable.Empty<Client>();
        Logger = logger;
    }

    /// <summary>
    /// Determines whether origin is allowed.
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <returns></returns>
    public virtual Task<bool> IsOriginAllowedAsync(string origin)
    {
        using var activity = Tracing.ActivitySource.StartActivity("InMemoryCorsPolicyService.IsOriginAllowedAsync");

        var query = Clients
            .SelectMany(x => x.AllowedCorsOrigins)
            .Select(x => x.GetOrigin());
        var result = query.Contains(origin, StringComparer.OrdinalIgnoreCase);

        if (result)
        {
            Logger.LogDebug("Client list checked and origin: {0} is allowed", origin);
        }
        else
        {
            Logger.LogDebug("Client list checked and origin: {0} is not allowed", origin);
        }

        return Task.FromResult(result);
    }
}