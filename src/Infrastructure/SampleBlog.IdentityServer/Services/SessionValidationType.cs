namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Represent the type of session validation request
/// </summary>
public enum SessionValidationType
{
    /// <summary>
    /// Refresh token use at token endpoint
    /// </summary>
    RefreshToken,

    /// <summary>
    /// Access token use by client at userinfo endpoint or at an API that uses introspection
    /// </summary>
    AccessToken
}