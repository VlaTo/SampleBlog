using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.Core;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// Default signing credentials store
/// </summary>
/// <seealso cref="ISigningCredentialStore" />
public sealed class InMemorySigningCredentialsStore : ISigningCredentialStore
{
    private readonly SigningCredentials credential;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemorySigningCredentialsStore"/> class.
    /// </summary>
    /// <param name="credential">The credential.</param>
    public InMemorySigningCredentialsStore(SigningCredentials credential)
    {
        this.credential = credential;
    }

    /// <summary>
    /// Gets the signing credentials.
    /// </summary>
    /// <returns></returns>
    public Task<SigningCredentials?> GetSigningCredentialsAsync()
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("InMemorySigningCredentialsStore.GetSigningCredentials");

        return Task.FromResult<SigningCredentials?>(credential);
    }
}