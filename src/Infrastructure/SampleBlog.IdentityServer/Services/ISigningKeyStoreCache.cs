using SampleBlog.IdentityServer.Services.KeyManagement;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Interface to model caching keys loaded from key store.
/// </summary>
public interface ISigningKeyStoreCache
{
    /// <summary>
    /// Returns cached keys.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<KeyContainer>?> GetKeysAsync();

    /// <summary>
    /// Caches keys for duration.
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    Task StoreKeysAsync(IEnumerable<KeyContainer> keys, TimeSpan duration);
}