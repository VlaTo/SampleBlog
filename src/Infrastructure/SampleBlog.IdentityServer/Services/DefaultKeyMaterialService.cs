using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// The default key material service
/// </summary>
/// <seealso cref="IKeyMaterialService" />
public class DefaultKeyMaterialService : IKeyMaterialService
{
    public DefaultKeyMaterialService()
    {
    }

    public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        using var activity = Tracing.ServiceActivitySource.StartActivity("DefaultKeyMaterialService.GetValidationKeys");

        var keys = new List<SecurityKeyInfo>();

        /*var automaticSigningKeys = await _keyManagerKeyStore.GetValidationKeysAsync();
        if (automaticSigningKeys?.Any() == true)
        {
            keys.AddRange(automaticSigningKeys);
        }

        foreach (var store in _validationKeysStores)
        {
            var validationKeys = await store.GetValidationKeysAsync();
            if (validationKeys.Any())
            {
                keys.AddRange(validationKeys);
            }
        }*/

        return Task.FromResult<IEnumerable<SecurityKeyInfo>>(keys);
    }

    public async Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string>? allowedAlgorithms = null)
    {
        using var activity = Tracing.ServiceActivitySource.StartActivity("DefaultKeyMaterialService.GetSigningCredentials");

        if (null == allowedAlgorithms)
        {
            /*var list = _signingCredentialStores.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var key = await list[i].GetSigningCredentialsAsync();
                if (key != null)
                {
                    return key;
                }
            }

            var automaticKey = await _keyManagerKeyStore.GetSigningCredentialsAsync();
            if (automaticKey != null)
            {
                return automaticKey;
            }*/

            throw new InvalidOperationException($"No signing credential registered.");
        }

        var credentials = await GetAllSigningCredentialsAsync();
        var credential = credentials.FirstOrDefault(signin => allowedAlgorithms.Contains(signin.Algorithm));

        if (credential is null)
        {
            throw new InvalidOperationException($"No signing credential for algorithms ({allowedAlgorithms.ToSpaceSeparatedString()}) registered.");
        }

        return credential;
    }

    public Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
    {
        var credentials = new List<SigningCredentials>();
        return Task.FromResult<IEnumerable<SigningCredentials>>(credentials);
    }
}