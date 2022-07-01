using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Endpoints.Results;
using SampleBlog.IdentityServer.Hosting;

namespace SampleBlog.IdentityServer.Endpoints;

internal sealed class CheckSessionEndpoint : IEndpointHandler
{
    private readonly ILogger logger;

    public CheckSessionEndpoint(ILogger<CheckSessionEndpoint> logger)
    {
        this.logger = logger;
    }

    public Task<IEndpointResult?> ProcessAsync(HttpContext context)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity(Constants.EndpointNames.CheckSession + "Endpoint");

        IEndpointResult result;

        if (!HttpMethods.IsGet(context.Request.Method))
        {
            logger.LogWarning("Invalid HTTP method for check session endpoint");
            result = new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }
        else
        {
            logger.LogDebug("Rendering check session result");
            result = new CheckSessionResult();
        }

        return Task.FromResult<IEndpointResult?>(result);
    }
}