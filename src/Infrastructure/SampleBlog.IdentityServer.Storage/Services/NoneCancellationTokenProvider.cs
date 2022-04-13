namespace SampleBlog.IdentityServer.Storage.Services;

/// <summary>
/// Implementation of ICancellationTokenProvider that returns CancellationToken.None
/// </summary>
public sealed class NoneCancellationTokenProvider : ICancellationTokenProvider
{
    /// <inheritdoc/>
    public CancellationToken CancellationToken => CancellationToken.None;
}