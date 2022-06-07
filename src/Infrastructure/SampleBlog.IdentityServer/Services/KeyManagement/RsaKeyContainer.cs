using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace SampleBlog.IdentityServer.Services.KeyManagement;

/// <summary>
/// Container class for RsaSecurityKey.
/// </summary>
public class RsaKeyContainer : KeyContainer
{
    /// <summary>
    /// The RSAParameters.
    /// </summary>
    public RSAParameters Parameters
    {
        get;
        set;
    }

    /// <summary>
    /// Constructor for RsaKeyContainer.
    /// </summary>
    public RsaKeyContainer()
        : base()
    {
    }

    /// <summary>
    /// Constructor for RsaKeyContainer.
    /// </summary>
    public RsaKeyContainer(RsaSecurityKey key, string algorithm, DateTime created)
        : base(key.KeyId, algorithm, created)
    {
        if (key.Rsa != null)
        {
            Parameters = key.Rsa.ExportParameters(includePrivateParameters: true);
        }
        else
        {
            Parameters = key.Parameters;
        }
    }

    /// <inheritdoc/>
    public override AsymmetricSecurityKey ToSecurityKey()
    {
        return new RsaSecurityKey(Parameters)
        {
            KeyId = Id
        };
    }
}