using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Implements IServerUrls
/// </summary>
public class DefaultServerUrls : IServerUrls
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// ctor
    /// </summary>
    public DefaultServerUrls(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public string? Origin
    {
        get
        {
            var request = httpContextAccessor.HttpContext?.Request;
            return null != request ? (request.Scheme + Uri.SchemeDelimiter + request.Host.ToUriComponent()) : null;
        }
        set
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return ;
            }

            //var split = UriParser. value.Split(new[] { Uri.SchemeDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            var request = httpContextAccessor.HttpContext?.Request;

            if (null != request)
            {
                var builder = new UriBuilder(value);

                request.Scheme = builder.Scheme; // split.First();
                request.Host = new HostString(builder.Host); // new HostString(split.Last());
            }
        }
    }

    /// <inheritdoc/>
    public string? BasePath
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            //return httpContextAccessor.HttpContext.Items[Constants.EnvironmentKeys.IdentityServerBasePath] as string;
            return context?.Items[Constants.EnvironmentKeys.IdentityServerBasePath] as string;
        }
        set
        {
            var context = httpContextAccessor.HttpContext;

            //httpContextAccessor.HttpContext.Items[Constants.EnvironmentKeys.IdentityServerBasePath] = value.RemoveTrailingSlash();

            if (null != context)
            {
                context.Items[Constants.EnvironmentKeys.IdentityServerBasePath] = value.RemoveTrailingSlash();
            }
        }
    }
}