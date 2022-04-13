using Microsoft.Extensions.Logging;

namespace SampleBlog.IdentityServer.Extensions;

public static class StringEnumerableExtensions
{
    public static string ToSpaceSeparatedString(this IEnumerable<string>? strings)
    {
        return null == strings ? String.Empty : String.Join(' ', strings);
    }

    internal static bool AreValidResourceIndicatorFormat(this IEnumerable<string>? list, ILogger logger)
    {
        if (null != list)
        {
            foreach (var item in list)
            {
                if (!Uri.IsWellFormedUriString(item, UriKind.Absolute))
                {
                    logger.LogDebug("Resource indicator {resource} is not a valid URI.", item);
                    return false;
                }

                if (item.Contains("#"))
                {
                    logger.LogDebug("Resource indicator {resource} must not contain a fragment component.", item);
                    return false;
                }
            }
        }

        return true;
    }
}