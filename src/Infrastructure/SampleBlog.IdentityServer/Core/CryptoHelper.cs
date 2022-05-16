using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace SampleBlog.IdentityServer.Core;

public class CryptoHelper
{
    /// <summary>
    /// Return the matching RFC 7518 crv value for curve
    /// </summary>
    internal static string GetCrvValueFromCurve(ECCurve curve)
    {
        return curve.Oid.Value switch
        {
            Constants.CurveOids.P256 => JsonWebKeyECTypes.P256,
            Constants.CurveOids.P384 => JsonWebKeyECTypes.P384,
            Constants.CurveOids.P521 => JsonWebKeyECTypes.P521,
            _ => throw new InvalidOperationException($"Unsupported curve type of {curve.Oid.Value} - {curve.Oid.FriendlyName}"),
        };
    }
}