namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Service responsible for logic around coordinating client and server session lifetimes.
/// </summary>
public interface ISessionCoordinationService
{
    /// <summary>
    /// Coordinates when a user logs out.
    /// </summary>
    Task ProcessLogoutAsync(UserSession session);

    /// <summary>
    /// Coordinates when a user session has expired.
    /// </summary>
    Task ProcessExpirationAsync(UserSession session);

    /// <summary>
    /// Validates client request, and if valid extends server-side session.
    /// Returns false if the session is invalid, true otherwise.
    /// </summary>
    Task<bool> ValidateSessionAsync(SessionValidationRequest request);
}