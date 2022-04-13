using Microsoft.AspNetCore.Http;

namespace SampleBlog.Identity.Authorization.Core;

internal sealed class AbsoluteUrlFactory : IAbsoluteUrlFactory
{
    public IHttpContextAccessor ContextAccessor
    {
        get;
    }

    public AbsoluteUrlFactory(IHttpContextAccessor httpContextAccessor)
    {
        // We need the context accessor here in order to produce an absolute url from a potentially relative url.
        ContextAccessor = httpContextAccessor;
    }

    // Call this method when you are overriding a service that doesn't have an HttpContext instance available.
    public string GetAbsoluteUrl(string path)
    {
        var (process, result) = ShouldProcessPath(path);

        if (false == process)
        {
            return result!;
        }

        if (null == ContextAccessor.HttpContext?.Request)
        {
            throw new InvalidOperationException("The request is not currently available. This service can only be used within the context of an existing HTTP request.");
        }

        return GetAbsoluteUrl(ContextAccessor.HttpContext, path);
    }

    // Call this method when you are implementing a service that has an HttpContext instance available.
    public string GetAbsoluteUrl(HttpContext context, string path)
    {
        var (process, result) = ShouldProcessPath(path);

        if (false == process)
        {
            return result!;
        }

        var request = context.Request;

        return $"{request.Scheme}://{request.Host.ToUriComponent()}{request.PathBase.ToUriComponent()}{path}";
    }

    private static (bool, string?) ShouldProcessPath(string? path)
    {
        if (null == path || false == Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
        {
            return (false, null);
        }

        if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
        {
            return (false, path);
        }

        return (true, path);
    }
}