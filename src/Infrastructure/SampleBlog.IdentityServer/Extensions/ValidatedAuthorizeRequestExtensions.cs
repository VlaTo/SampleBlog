using System.Security.Cryptography;
using System.Text;
using IdentityModel;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Extensions;

public static class ValidatedAuthorizeRequestExtensions
{
    public static string? GetIdP(this ValidatedAuthorizeRequest request)
    {
        return request.GetPrefixedAcrValue(Constants.KnownAcrValues.HomeRealm);
    }

    public static string? GetTenant(this ValidatedAuthorizeRequest request)
    {
        return request.GetPrefixedAcrValue(Constants.KnownAcrValues.Tenant);
    }

    public static string? GetPrefixedAcrValue(this ValidatedAuthorizeRequest request, string prefix)
    {
        var value = request.AuthenticationContextReferenceClasses
            .FirstOrDefault(x => x.StartsWith(prefix));
        return value?.Substring(prefix.Length);
    }

    public static IEnumerable<string> GetAcrValues(this ValidatedAuthorizeRequest request)
    {
        return request
            .AuthenticationContextReferenceClasses
            .Where(acr => !Constants.KnownAcrValues.All.Any(acr.StartsWith))
            .Distinct()
            .ToArray();
    }

    public static void RemoveIdP(this ValidatedAuthorizeRequest request)
    {
        request.RemovePrefixedAcrValue(Constants.KnownAcrValues.HomeRealm);
    }

    public static string? GenerateSessionStateValue(this ValidatedAuthorizeRequest? request)
    {
        if (null == request)
        {
            return null;
        }

        if (false == request.IsOpenIdRequest)
        {
            return null;
        }

        if (null == request.SessionId)
        {
            return null;
        }

        if (request.ClientId.IsMissing())
        {
            return null;
        }

        if (request.RedirectUri.IsMissing())
        {
            return null;
        }

        var clientId = request.ClientId;
        var sessionId = request.SessionId;
        var salt = CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex);

        var uri = new Uri(request.RedirectUri);
        var origin = uri.Scheme + "://" + uri.Host;
        
        if (!uri.IsDefaultPort)
        {
            origin += ":" + uri.Port;
        }

        var bytes = Encoding.UTF8.GetBytes(clientId + origin + sessionId + salt);
        byte[] hash;

        using (var sha = SHA256.Create())
        {
            hash = sha.ComputeHash(bytes);
        }

        return Base64Url.Encode(hash) + "." + salt;
    }

    public static void RemovePrefixedAcrValue(this ValidatedAuthorizeRequest request, string prefix)
    {
        request.AuthenticationContextReferenceClasses
            .RemoveAll(acr => acr.StartsWith(prefix, StringComparison.Ordinal));

        var acr_values = request.AuthenticationContextReferenceClasses.ToSpaceSeparatedString();

        if (acr_values.IsPresent())
        {
            request.Raw[OidcConstants.AuthorizeRequest.AcrValues] = acr_values;
        }
        else
        {
            request.Raw.Remove(OidcConstants.AuthorizeRequest.AcrValues);
        }
    }
}