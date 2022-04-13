using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.Services;

namespace SampleBlog.IdentityServer.Extensions;

public static class ServerUrlsExtensions
{
    /// <summary>
    /// Returns the origin in unicode, and not in punycode (if we have a unicode hostname)
    /// </summary>
    public static string? GetUnicodeOrigin(this IServerUrls urls)
    {
        const string schemaDelimiter = "://";
        var split = urls.Origin?.Split(new[] { schemaDelimiter }, StringSplitOptions.RemoveEmptyEntries);

        if (null != split)
        {
            var scheme = split.First();
            var host = HostString.FromUriComponent(split.Last()).Value;

            return scheme + schemaDelimiter + host;
        }

        return urls.Origin;
    }

    /// <summary>
    /// Returns an absolute URL for the URL or path.
    /// </summary>
    public static string GetAbsoluteUrl(this IServerUrls urls, string? urlOrPath)
    {
        if (urlOrPath.IsLocalUrl())
        {
            if (urlOrPath.StartsWith("~/"))
            {
                urlOrPath = urlOrPath.Substring(1);
            }

            return urls.BaseUrl + urlOrPath.EnsureLeadingSlash();
        }

        return urlOrPath ?? urls.BaseUrl;
    }
}