using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SampleBlog.IdentityServer.Services;

namespace SampleBlog.IdentityServer.Hosting;

public class BaseUrlMiddleware
{
    private readonly RequestDelegate next;

    public BaseUrlMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var urls = context.RequestServices.GetRequiredService<IServerUrls>();

        urls.BasePath = context.Request.PathBase.Value;

        await next(context);
    }
}