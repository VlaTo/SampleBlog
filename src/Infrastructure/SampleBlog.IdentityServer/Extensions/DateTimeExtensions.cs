using System.Diagnostics;

namespace SampleBlog.IdentityServer.Extensions;

internal static class DateTimeExtensions
{
    [DebuggerStepThrough]
    public static bool HasExceeded(this DateTime creationTime, TimeSpan timeout, DateTime now)
    {
        return now > (creationTime + timeout);
    }
}