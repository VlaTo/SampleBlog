using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;

namespace SampleBlog.IdentityServer.Endpoints.Results;

internal sealed class UserInfoResult : IEndpointResult
{
    public Dictionary<string, object> Claims
    {
        get;
    }

    public UserInfoResult(Dictionary<string, object> claims)
    {
        Claims = claims;
    }

    public Task ExecuteAsync(HttpContext context)
    {
        context.Response.SetNoCache();
        return context.Response.WriteJsonAsync(Claims);
    }
}