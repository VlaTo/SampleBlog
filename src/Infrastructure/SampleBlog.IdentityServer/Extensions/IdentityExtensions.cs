using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using IdentityModel;

namespace SampleBlog.IdentityServer.Extensions;

internal static class IdentityExtensions
{
    /// <summary>
    /// Gets the identity provider.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">idp claim is missing</exception>
    [DebuggerStepThrough]
    public static string? GetIdentityProvider(this IIdentity? identity)
    {
        if (identity is ClaimsIdentity id)
        {
            var claim = id.FindFirst(JwtClaimTypes.IdentityProvider);

            if (null == claim)
            {
                throw new InvalidOperationException("idp claim is missing");
            }

            return claim.Value;
        }

        return null;
    }

    /// <summary>
    /// Gets the authentication epoch time.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static long GetAuthenticationTimeEpoch(this IIdentity identity)
    {
        if (identity is ClaimsIdentity id)
        {
            var claim = id.FindFirst(JwtClaimTypes.AuthenticationTime);

            if (claim == null) throw new InvalidOperationException("auth_time is missing.");

            return long.Parse(claim.Value);
        }

        return 0L;
    }
}