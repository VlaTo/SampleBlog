using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Core;

internal class TokenRequestValidationLog
{
    public string ClientId
    {
        get;
        set;
    }

    public string ClientName
    {
        get;
        set;
    }

    public string GrantType
    {
        get;
        set;
    }

    public string Scopes
    {
        get;
        set;
    }

    public string AuthorizationCode
    {
        get;
        set;
    }

    public string RefreshToken
    {
        get;
        set;
    }

    public string UserName
    {
        get;
        set;
    }

    public IEnumerable<string> AuthenticationContextReferenceClasses
    {
        get;
        set;
    }

    public string Tenant
    {
        get;
        set;
    }

    public string IdP
    {
        get;
        set;
    }

    public Dictionary<string, string> Raw
    {
        get;
        set;
    }

    public TokenRequestValidationLog(ValidatedTokenRequest request, IEnumerable<string> sensitiveValuesFilter)
    {
        Raw = request.Raw.ToScrubbedDictionary(sensitiveValuesFilter.ToArray());

        if (null != request.Client)
        {
            ClientId = request.Client.ClientId;
            ClientName = request.Client.ClientName;
        }

        if (null != request.RequestedScopes)
        {
            Scopes = request.RequestedScopes.ToSpaceSeparatedString();
        }

        GrantType = request.GrantType;
        AuthorizationCode = request.AuthorizationCodeHandle.Obfuscate();
        RefreshToken = request.RefreshTokenHandle.Obfuscate();
        UserName = request.UserName;
    }

    /*public override string ToString()
    {
        return LogSerializer.Serialize(this);
    }*/
}