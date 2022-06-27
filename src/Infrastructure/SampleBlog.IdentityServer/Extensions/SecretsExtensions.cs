using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using SampleBlog.IdentityServer.Storage.Models;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace SampleBlog.IdentityServer.Extensions;

public static class SecretsExtensions
{
    /// <summary>
    /// Constructs a list of SecurityKey from a Secret collection
    /// </summary>
    /// <param name="secrets">The secrets</param>
    /// <returns></returns>
    public static Task<List<SecurityKey>> GetKeysAsync(this IEnumerable<Secret> secrets)
    {
        var secretList = secrets.ToList().AsReadOnly();
        var keys = new List<SecurityKey>();

        var certificates = GetCertificates(secretList)
            .Select(c => (SecurityKey)new X509SecurityKey(c))
            .ToList();

        keys.AddRange(certificates);

        var jwks = secretList
            .Where(s => s.Type == IdentityServerConstants.SecretTypes.JsonWebKey)
            .Select(s => new JsonWebKey(s.Value))
            .ToList();
        keys.AddRange(jwks);

        return Task.FromResult(keys);
    }

    private static ICollection<X509Certificate2> GetCertificates(IEnumerable<Secret> secrets)
    {
        return secrets
            .Where(s => s.Type == IdentityServerConstants.SecretTypes.X509CertificateBase64)
            .Select(s => new X509Certificate2(Convert.FromBase64String(s.Value)))
            .Where(c => c != null)
            .ToList();
    }
}