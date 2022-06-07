using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Services.KeyManagement;

/// <summary>
/// Implementation of IValidationKeysStore and ISigningCredentialStore based on KeyManager.
/// </summary>
public class AutomaticKeyManagerKeyStore : IAutomaticKeyManagerKeyStore
{
    private readonly IKeyManager keyManager;
    private readonly KeyManagementOptions options;

    /// <summary>
    /// Constructor for KeyManagerKeyStore.
    /// </summary>
    /// <param name="keyManager"></param>
    /// <param name="options"></param>
    public AutomaticKeyManagerKeyStore(IKeyManager keyManager, KeyManagementOptions options)
    {
        this.keyManager = keyManager;
        this.options = options;
    }

    /// <inheritdoc/>
    public async Task<SigningCredentials?> GetSigningCredentialsAsync()
    {
        if (false == options.Enabled)
        {
            return null;
        }

        var credentials = await GetAllSigningCredentialsAsync();

        var algorithm = options.DefaultSigningAlgorithm;
        var credential = credentials.FirstOrDefault(x => String.Equals(algorithm, x.Algorithm));

        return credential;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
    {
        if (false == options.Enabled)
        {
            return Enumerable.Empty<SigningCredentials>();
        }

        var containers = await keyManager.GetCurrentKeysAsync();

        var credentials = containers.Select(
            x => new SigningCredentials(x.ToSecurityKey(), x.Algorithm)
        );
        
        return credentials;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        if (false == options.Enabled)
        {
            return Enumerable.Empty<SecurityKeyInfo>();
        }

        var containers = await keyManager.GetAllKeysAsync();

        var keys = containers.Select(x => new SecurityKeyInfo
        {
            Key = x.ToSecurityKey(),
            SigningAlgorithm = x.Algorithm
        });

        return keys.ToArray();
    }
}