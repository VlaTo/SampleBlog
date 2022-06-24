using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace SampleBlog.IdentityServer.Extensions;

public static class PrincipalExtensions
{
    /// <summary>
    /// Gets the identity provider.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string? GetIdentityProvider(this IPrincipal? principal)
    {
        return principal?.Identity?.GetIdentityProvider();
    }

    /// <summary>
    /// Gets the authentication method claims.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static IEnumerable<Claim> GetAuthenticationMethods(this IPrincipal principal)
    {
        return principal.Identity.GetAuthenticationMethods();
    }

    /// <summary>
    /// Gets the authentication time.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static DateTime GetAuthenticationTime(this IPrincipal? principal)
    {
        return null != principal
            ? DateTimeOffset.FromUnixTimeSeconds(principal.GetAuthenticationTimeEpoch()).UtcDateTime
            : DateTime.MinValue;
    }

    /// <summary>
    /// Gets the authentication epoch time.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static long GetAuthenticationTimeEpoch(this IPrincipal principal)
    {
        return principal.Identity?.GetAuthenticationTimeEpoch() ?? 0L;
    }
}