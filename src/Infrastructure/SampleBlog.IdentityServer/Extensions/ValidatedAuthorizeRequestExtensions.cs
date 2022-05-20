using System.Collections.Specialized;
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
    
    public static string ToOptimizedQueryString(this ValidatedAuthorizeRequest request)
    {
        return request.ToOptimizedRawValues().ToQueryString();
    }

    public static IDictionary<string, string[]> ToOptimizedFullDictionary(this ValidatedAuthorizeRequest request)
    {
        return request.ToOptimizedRawValues().ToFullDictionary();
    }
    
    public static void RemovePrompt(this ValidatedAuthorizeRequest request)
    {
        var suppress = new StringBuilder();

        if (request.PromptModes.Contains(OidcConstants.PromptModes.Login))
        {
            suppress.Append(OidcConstants.PromptModes.Login);
        }

        if (request.PromptModes.Contains(OidcConstants.PromptModes.SelectAccount))
        {
            if (0 < suppress.Length)
            {
                suppress.Append(' ');
            }

            suppress.Append(OidcConstants.PromptModes.SelectAccount);
        }

        request.Raw.Add(Constants.SuppressedPrompt, suppress.ToString());
        request.PromptModes = request.PromptModes
            .Except(new[]
            {
                OidcConstants.PromptModes.Login,
                OidcConstants.PromptModes.SelectAccount
            })
            .ToArray();
    }

    private static NameValueCollection ToOptimizedRawValues(this ValidatedAuthorizeRequest request)
    {
        if (request.Raw.AllKeys.Contains(OidcConstants.AuthorizeRequest.Request))
        {
            // if we already have a request object in the URL, then we can filter out the duplicate entries in the Raw collection
            var collection = new NameValueCollection();

            foreach (var key in request.Raw.AllKeys)
            {
                // https://openid.net/specs/openid-connect-core-1_0.html#JWTRequests 
                // requires client id and response type to always be in URL
                if (key == OidcConstants.AuthorizeRequest.ClientId ||
                    key == OidcConstants.AuthorizeRequest.ResponseType ||
                    request.RequestObjectValues.All(x => x.Type != key))
                {
                    collection.Add(key, request.Raw[key]);
                }
            }

            return collection;
        }

        return request.Raw;
    }
}