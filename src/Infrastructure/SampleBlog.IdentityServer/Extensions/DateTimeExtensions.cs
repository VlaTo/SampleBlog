using System.Diagnostics;

namespace SampleBlog.IdentityServer.Extensions;

internal static class DateTimeExtensions
{
    [DebuggerStepThrough]
    public static bool HasExceeded(this DateTime creationTime, TimeSpan timeout, DateTime now)
    {
        return now > (creationTime + timeout);
    }

    [DebuggerStepThrough]
    public static bool HasExpired(this DateTime? expirationTime, DateTime now)
    {
        return expirationTime.HasValue && expirationTime.Value.HasExpired(now);
    }

    [DebuggerStepThrough]
    public static bool HasExpired(this DateTime expirationTime, DateTime now)
    {
        return now > expirationTime;
    }

    [DebuggerStepThrough]
    public static TimeSpan GetLifetimeInSeconds(this DateTime creationTime, DateTime now)
    {
        //return ((int)(now - creationTime).TotalSeconds);
        return now - creationTime;
    }
}