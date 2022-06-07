namespace SampleBlog.IdentityServer.Core;

/// <summary>
/// Nop implementation.
/// </summary>
public class NopConcurrencyLock<T> : IConcurrencyLock<T>
{
    /// <inheritdoc/>
    public Task<bool> LockAsync(TimeSpan timeout)
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public void Unlock()
    {
    }
}