using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// The default validation key store
/// </summary>
/// <seealso cref="IValidationKeysStore" />
public sealed class InMemoryValidationKeysStore : IValidationKeysStore
{
    private readonly IEnumerable<SecurityKeyInfo> keys;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryValidationKeysStore"/> class.
    /// </summary>
    /// <param name="keys">The keys.</param>
    /// <exception cref="System.ArgumentNullException">keys</exception>
    public InMemoryValidationKeysStore(IEnumerable<SecurityKeyInfo> keys)
    {
        this.keys = keys ?? throw new ArgumentNullException(nameof(keys));
    }

    /// <summary>
    /// Gets all validation keys.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("InMemoryValidationKeysStore.GetValidationKeys");

        return Task.FromResult(keys);
    }
}