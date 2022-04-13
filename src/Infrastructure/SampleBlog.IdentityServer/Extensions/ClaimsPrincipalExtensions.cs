using IdentityModel;
using System.Diagnostics;
using System.Security.Claims;

namespace SampleBlog.IdentityServer.Extensions;

internal static class ClaimsPrincipalExtensions
{
    public static string GetSubjectId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(JwtClaimTypes.Subject);
        
        if (null == claim)
        {
            throw new InvalidOperationException("Missing subject id for principal in authentication ticket.");
        }

        return claim.Value;
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string? GetDisplayName(this ClaimsPrincipal principal)
    {
        var name = principal.Identity?.Name;

        if (name.IsPresent())
        {
            return name;
        }

        var claim = principal.FindFirst(JwtClaimTypes.Subject);

        return null != claim ? claim.Value : String.Empty;
    }

    /// <summary>
    /// Determines whether this instance is authenticated.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns>
    ///   <c>true</c> if the specified principal is authenticated; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsAuthenticated(this ClaimsPrincipal? principal)
    {
        return principal is { Identity: { IsAuthenticated: true } };
    }
}