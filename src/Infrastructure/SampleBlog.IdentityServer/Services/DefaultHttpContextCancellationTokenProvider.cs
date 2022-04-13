using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.Storage.Services;

namespace SampleBlog.IdentityServer.Services;

public class DefaultHttpContextCancellationTokenProvider : ICancellationTokenProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CancellationToken CancellationToken => httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

    public DefaultHttpContextCancellationTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }
}