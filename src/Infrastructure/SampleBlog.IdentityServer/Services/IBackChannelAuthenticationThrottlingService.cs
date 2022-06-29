using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// The backchannel authentication throttling service.
/// </summary>
public interface IBackChannelAuthenticationThrottlingService
{
    /// <summary>
    /// Decides if the requesting client and request needs to slow down.
    /// </summary>
    Task<bool> ShouldSlowDown(string requestId, BackChannelAuthenticationRequest details);
}