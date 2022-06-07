using Microsoft.AspNetCore.Authentication;

namespace SampleBlog.IdentityServer.Extensions;

public static class SystemClockExtensions
{
    internal static TimeSpan GetAge(this ISystemClock clock, DateTime date)
    {
        var now = clock.UtcNow.UtcDateTime;

        if (date > now)
        {
            now = date;
        }

        return now.Subtract(date);
    }
}