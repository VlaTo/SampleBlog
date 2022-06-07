namespace SampleBlog.IdentityServer.Core;

/// <summary>
/// Interface to model locking.
/// </summary>
public interface IConcurrencyLock<T>
{
    /// <summary>
    /// Locks. Returns false if lock was not obtained within in the timeout.
    /// </summary>
    /// <returns></returns>
    Task<bool> LockAsync(TimeSpan timeout);

    /// <summary>
    /// Unlocks
    /// </summary>
    /// <returns></returns>
    void Unlock();
}