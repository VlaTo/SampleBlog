namespace SampleBlog.IdentityServer.Core;

/// <summary>
/// Default implementation.
/// </summary>
public class DefaultConcurrencyLock<T> : IConcurrencyLock<T>
{
    private static readonly SemaphoreSlim Lock = new(1);

    /// <inheritdoc/>
    public Task<bool> LockAsync(TimeSpan timeout)
    {
        if (TimeSpan.Zero > timeout)
        {
            throw new ArgumentException("millisecondsTimeout must be greater than zero.");
        }

        return Lock.WaitAsync(timeout);
    }

    /// <inheritdoc/>
    public void Unlock()
    {
        Lock.Release();
    }
}