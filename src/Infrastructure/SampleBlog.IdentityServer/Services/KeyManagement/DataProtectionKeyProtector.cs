using Microsoft.AspNetCore.DataProtection;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Services.KeyManagement;

/// <summary>
/// Implementation of IKeyProtector based on ASP.NET Core's data protection feature.
/// </summary>
public class DataProtectionKeyProtector : ISigningKeyProtector
{
    private readonly KeyManagementOptions options;
    private readonly IDataProtector dataProtectionProvider;

    /// <summary>
    /// The X509 certificate data.
    /// </summary>
    public string CertificateRawData
    {
        get;
        set;
    }

    /// <summary>
    /// Constructor for DataProtectionKeyProtector.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dataProtectionProvider"></param>
    public DataProtectionKeyProtector(
        KeyManagementOptions options,
        IDataProtectionProvider dataProtectionProvider)
    {
        this.options = options;
        this.dataProtectionProvider = dataProtectionProvider.CreateProtector(nameof(DataProtectionKeyProtector));
    }

    /// <inheritdoc/>
    public SerializedKey Protect(KeyContainer key)
    {
        var data = KeySerializer.Serialize(key);

        if (options.DataProtectKeys)
        {
            data = dataProtectionProvider.Protect(data);
        }

        return new SerializedKey
        {
            Version = 1,
            Created = DateTime.UtcNow,
            Id = key.Id,
            Algorithm = key.Algorithm,
            IsX509Certificate = key.HasX509Certificate,
            Data = data,
            DataProtected = options.DataProtectKeys
        };
    }

    /// <inheritdoc/>
    public KeyContainer Unprotect(SerializedKey key)
    {
        var data = key.DataProtected ?
            dataProtectionProvider.Unprotect(key.Data) :
            key.Data;

        if (key.IsX509Certificate)
        {
            return KeySerializer.Deserialize<X509KeyContainer>(data);
        }

        if (key.Algorithm.StartsWith("R") || key.Algorithm.StartsWith("P"))
        {
            return KeySerializer.Deserialize<RsaKeyContainer>(data);
        }

        if (key.Algorithm.StartsWith("E"))
        {
            return KeySerializer.Deserialize<EcKeyContainer>(data);
        }

        throw new Exception($"Invalid Algorithm: {key.Algorithm} for kid: {key.Id}");
    }
}