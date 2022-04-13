using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Models persistence of events
/// </summary>
public interface IEventSink
{
    /// <summary>
    /// Raises the specified event.
    /// </summary>
    /// <param name="evt">The event.</param>
    Task PersistAsync(Event evt);
}