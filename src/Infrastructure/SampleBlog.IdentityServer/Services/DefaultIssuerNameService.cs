using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Abstracts issuer name access
/// </summary>
public class DefaultIssuerNameService : IIssuerNameService
{
    private readonly IdentityServerOptions options;
    private readonly IServerUrls urls;
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="urls"></param>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    public DefaultIssuerNameService(
        IdentityServerOptions options,
        IServerUrls urls,
        IHttpContextAccessor httpContextAccessor)
    {
        this.options = options;
        this.urls = urls;
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Task<string> GetCurrentAsync()
    {
        // if they've explicitly configured a URI then use it,
        // otherwise dynamically calculate it
        var issuer = options.IssuerUri;

        if (null == issuer)
        {
            string? origin = null;

            if (options.MutualTls.Enabled && options.MutualTls.DomainName.IsPresent())
            {
                if (false == options.MutualTls.DomainName.Contains("."))
                {
                    var request = httpContextAccessor.HttpContext?.Request;

                    if (null != request && request.Host.Value.StartsWith(options.MutualTls.DomainName, StringComparison.OrdinalIgnoreCase))
                    {
                        // if MTLS is configured with domain like "foo", then the request will be for "foo.acme.com", 
                        // so the issuer we use is from the parent domain (e.g. "acme.com")
                        // 
                        // Host.Value is used to get unicode hostname, instread of ToUriComponent (aka punycode)

                        origin = request.Scheme + "://" + request.Host.Value.Substring(options.MutualTls.DomainName.Length + 1);
                    }
                }
            }

            if (null == origin)
            {
                // no MTLS, so use the current origin for the issuer
                // this also means we emit the issuer value in unicode
                origin = urls.GetUnicodeOrigin();
            }

            issuer = origin + urls.BasePath;

            if (options.LowerCaseIssuerUri)
            {
                issuer = issuer.ToLowerInvariant();
            }
        }

        return Task.FromResult(issuer);
    }
}