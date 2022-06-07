using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.Stores;

namespace SampleBlog.IdentityServer.Services.KeyManagement;

/// <summary>
/// Store abstraction for automatic key management.
/// </summary>
public interface IAutomaticKeyManagerKeyStore : IValidationKeysStore, ISigningCredentialStore
{
    /// <summary>
    /// Gets all the signing credentials.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync();
}