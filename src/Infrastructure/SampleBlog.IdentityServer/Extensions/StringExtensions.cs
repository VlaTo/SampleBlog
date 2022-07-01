using System.Diagnostics;
using System.Text.Encodings.Web;

namespace SampleBlog.IdentityServer.Extensions;

internal static class StringExtensions
{
    private const string SlashStr = "/";

    public static string? GetOrigin(this string? url)
    {
        if (null != url)
        {
            Uri uri;

            try
            {
                uri = new Uri(url);
            }
            catch (Exception)
            {
                return null;
            }

            return $"{uri.Scheme}://{uri.Authority}";
        }

        return null;
    }

    [DebuggerStepThrough]
    public static bool IsPresent(this string? value)
    {
        return !String.IsNullOrWhiteSpace(value);
    }

    [DebuggerStepThrough]
    public static bool IsMissing(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    [DebuggerStepThrough]
    public static string CleanUrlPath(this string? url)
    {
        if (String.IsNullOrWhiteSpace(url))
        {
            url = SlashStr;
        }

        if (url != SlashStr && url.EndsWith(SlashStr))
        {
            url = url.Substring(0, url.Length - 1);
        }

        return url;
    }

    [DebuggerStepThrough]
    public static string? EnsureLeadingSlash(this string? url)
    {
        return url != null && !url.StartsWith(SlashStr) ? SlashStr + url : url;
    }

    [DebuggerStepThrough]
    public static string? EnsureTrailingSlash(this string? url)
    {
        return url != null && !url.EndsWith(SlashStr) ? url + SlashStr : url;
    }

    [DebuggerStepThrough]
    public static string? RemoveLeadingSlash(this string? url)
    {
        if (url != null && url.StartsWith(SlashStr))
        {
            url = url.Substring(1);
        }

        return url;
    }

    [DebuggerStepThrough]
    public static string? RemoveTrailingSlash(this string? url)
    {
        if (url != null && url.EndsWith(SlashStr))
        {
            url = url.Substring(0, url.Length - 1);
        }

        return url;
    }

    public static List<string> ParseScopesString(this string? scopes)
    {
        var parsedScopes = new List<string>();

        if (false == scopes.IsMissing())
        {
            parsedScopes.AddRange(scopes
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
            );

            if (0 < parsedScopes.Count)
            {
                parsedScopes.Sort();
            }
        }

        return parsedScopes;
    }

    [DebuggerStepThrough]
    public static IEnumerable<string> FromSpaceSeparatedString(this string input)
    {
        return input
            .Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }

    [DebuggerStepThrough]
    public static bool IsLocalUrl(this string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // Allows "/" or "/foo" but not "//" or "/\".
        if (url[0] == '/')
        {
            // url is exactly "/"
            if (1 == url.Length)
            {
                return true;
            }

            // url doesn't start with "//" or "/\"
            if (url[1] != '/' && url[1] != '\\')
            {
                return true;
            }

            return false;
        }

        // Allows "~/" or "~/foo" but not "~//" or "~/\".
        if (url[0] == '~' && url.Length > 1 && url[1] == '/')
        {
            // url is exactly "~/"
            if (2 == url.Length)
            {
                return true;
            }

            // url doesn't start with "~//" or "~/\"
            if (url[2] != '/' && url[2] != '\\')
            {
                return true;
            }

            return false;
        }

        return false;
    }

    [DebuggerStepThrough]
    public static string AddQueryString(this string url, string query)
    {
        if (false == url.Contains("?"))
        {
            url += "?";
        }
        else if (false == url.EndsWith("&"))
        {
            url += "&";
        }

        return url + query;
    }

    [DebuggerStepThrough]
    public static string AddQueryString(this string url, string name, string? value)
    {
        return null != value
            ? url.AddQueryString(name + "=" + UrlEncoder.Default.Encode(value))
            : url;
    }

    [DebuggerStepThrough]
    public static string AddHashFragment(this string url, string query)
    {
        if (false == url.Contains("#"))
        {
            url += "#";
        }

        return url + query;
    }

    public static string Obfuscate(this string value)
    {
        var last4Chars = "****";

        if (false == String.IsNullOrEmpty(value) && value.Length > 4)
        {
            last4Chars = value.Substring(value.Length - 4);
        }

        return "****" + last4Chars;
    }
}