using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.Hosting;

internal sealed class EndpointRouter : IEndpointRouter
{
    private readonly IEnumerable<Endpoint> endpoints;
    private readonly IdentityServerOptions options;
    private readonly ILogger logger;

    public EndpointRouter(IEnumerable<Endpoint> endpoints, IdentityServerOptions options, ILogger<EndpointRouter> logger)
    {
        this.endpoints = endpoints;
        this.options = options;
        this.logger = logger;
    }

    public IEndpointHandler? Find(HttpContext context)
    {
        if (null == context)
        {
            throw new ArgumentNullException(nameof(context));
        }

        foreach (var endpoint in endpoints)
        {
            var path = endpoint.Path;

            if (context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                var endpointName = endpoint.Name;
                
                logger.LogDebug("Request path {path} matched to endpoint type {endpoint}", context.Request.Path, endpointName);

                return GetEndpointHandler(endpoint, context);
            }
        }

        logger.LogTrace("No endpoint entry found for request path: {path}", context.Request.Path);

        return null;
    }

    private IEndpointHandler? GetEndpointHandler(Endpoint endpoint, HttpContext context)
    {
        if (options.Endpoints.IsEndpointEnabled(endpoint))
        {
            if (context.RequestServices.GetService(endpoint.Handler) is IEndpointHandler handler)
            {
                logger.LogDebug("Endpoint enabled: {endpoint}, successfully created handler: {endpointHandler}", endpoint.Name, endpoint.Handler.FullName);
                return handler;
            }

            logger.LogDebug("Endpoint enabled: {endpoint}, failed to create handler: {endpointHandler}", endpoint.Name, endpoint.Handler.FullName);
        }
        else
        {
            logger.LogWarning("Endpoint disabled: {endpoint}", endpoint.Name);
        }

        return null;
    }
}