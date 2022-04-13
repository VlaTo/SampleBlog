using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SampleBlog.IdentityServer.DependencyInjection.Options;

namespace SampleBlog.IdentityServer.Hosting;

internal sealed class DynamicSchemeAuthenticationMiddleware
{
    private readonly RequestDelegate next;
    private readonly DynamicProviderOptions options;

    public DynamicSchemeAuthenticationMiddleware(RequestDelegate next, DynamicProviderOptions options)
    {
        this.next = next;
        this.options = options;
    }

    public async Task Invoke(HttpContext context)
    {
        // this is needed to dynamically load the handler if this load balanced server
        // was not the one that initiated the call out to the provider
        if (context.Request.Path.StartsWithSegments(options.PathPrefix))
        {
            var startIndex = options.PathPrefix.ToString().Length;
            var scheme = context.Request.Path.Value.Substring(startIndex + 1);
            var idx = scheme.IndexOf('/');

            if (0 < idx)
            {
                // this assumes the path is: /<PathPrefix>/<scheme>/<extra>
                // e.g.: /federation/my-oidc-provider/signin
                scheme = scheme.Substring(0, idx);
            }

            var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

            if (await handlers.GetHandlerAsync(context, scheme) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
            {
                return;
            }
        }

        await next.Invoke(context);
    }
}