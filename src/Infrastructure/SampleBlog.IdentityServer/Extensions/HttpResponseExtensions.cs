using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;

namespace SampleBlog.IdentityServer.Extensions;

internal static class HttpResponseExtensions
{
    public static void AddScriptCspHeaders(this HttpResponse response, CspOptions options, string hash)
    {
        var csp1part = options.Level == CspLevel.One ? "'unsafe-inline' " : string.Empty;
        var cspHeader = $"default-src 'none'; script-src {csp1part}'{hash}'";

        AddCspHeaders(response.Headers, options, cspHeader);
    }

    public static void AddCspHeaders(IHeaderDictionary headers, CspOptions options, string cspHeader)
    {
        if (false == headers.ContainsKey("Content-Security-Policy"))
        {
            headers.Add("Content-Security-Policy", cspHeader);
        }

        if (options.AddDeprecatedHeader && false == headers.ContainsKey("X-Content-Security-Policy"))
        {
            headers.Add("X-Content-Security-Policy", cspHeader);
        }
    }

    public static void SetCache(this HttpResponse response, int maxAge, params string[]? varyBy)
    {
        if (0 == maxAge)
        {
            SetNoCache(response);
        }
        else if (0 < maxAge)
        {
            if (false == response.Headers.ContainsKey(HeaderNames.CacheControl))
            {
                var cacheControl = new CacheControlHeaderValue
                {
                    MaxAge = TimeSpan.FromSeconds(maxAge)
                };
                var value = new StringValues(cacheControl.ToString());
                response.Headers.Add(HeaderNames.CacheControl, value);
            }

            if (true == varyBy?.Any())
            {
                var vary = new StringValues(varyBy);
                
                if (response.Headers.ContainsKey(HeaderNames.Vary))
                {
                    vary = StringValues.Concat(response.Headers.Vary, vary);
                }

                response.Headers[HeaderNames.Vary] = vary;
            }
        }
    }

    public static void SetNoCache(this HttpResponse response)
    {
        var noCache = new CacheControlHeaderValue
            {
                NoStore = true,
                NoCache = true,
                MaxAge = TimeSpan.Zero
            }
            .ToString();

        /*if (false == response.Headers.ContainsKey(HeaderNames.CacheControl))
        {

            response.Headers.Add("Cache-Control", "no-store, no-cache, max-age=0");
        }
        else
        {
            response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        }*/

        response.Headers[HeaderNames.CacheControl] = new StringValues(noCache);

        if (false == response.Headers.ContainsKey(HeaderNames.Pragma))
        {
            response.Headers.Add(HeaderNames.Pragma, CacheControlHeaderValue.NoCacheString);
        }
    }

    public static async Task WriteHtmlAsync(this HttpResponse response, string html)
    {
        var encoding = Encoding.UTF8;
        var contentType = new ContentType(MediaTypeNames.Text.Html)
        {
            CharSet = encoding.HeaderName
        };

        response.ContentType = contentType.ToString();
        
        await response.WriteAsync(html, encoding);
        await response.Body.FlushAsync();
    }

    public static async Task WriteJsonAsync(this HttpResponse response, object obj, string? contentType = null)
    {
        var encoding = Encoding.UTF8;
        var json = ObjectSerializer.ToString(obj);
        var ct = new ContentType(contentType ?? MediaTypeNames.Application.Json)
        {
            CharSet = encoding.HeaderName
        };

        response.ContentType = ct.ToString();

        await response.WriteAsync(json, encoding);
        await response.Body.FlushAsync();
    }
}