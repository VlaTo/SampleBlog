using Microsoft.AspNetCore.Builder;

namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Options for the IdentityServer middleware
/// </summary>
public class IdentityServerMiddlewareOptions
{
    /// <summary>
    /// Callback to wire up an authentication middleware
    /// </summary>
    public Action<IApplicationBuilder> AuthenticationMiddleware
    {
        get;
        set;
    }

    public IdentityServerMiddlewareOptions()
    {
        AuthenticationMiddleware = (app) => app.UseAuthentication();
    }
}