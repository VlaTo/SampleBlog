using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SampleBlog.IdentityServer.EntityFramework.Storage;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.Storage.Models;
using ClientClaim = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.ClientClaim;

[Table(Database.Tables.Client, Schema = Database.Schemas.Identity)]
public class Client
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id
    {
        get;
        set;
    }

    public bool Enabled
    {
        get;
        set;
    }

    public string ClientId
    {
        get;
        set;
    }

    public string ProtocolType
    {
        get;
        set;
    }

    public List<ClientSecret> ClientSecrets
    {
        get;
        set;
    }

    public bool RequireClientSecret
    {
        get;
        set;
    }

    public string ClientName
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public string ClientUri
    {
        get;
        set;
    }

    public string LogoUri
    {
        get;
        set;
    }

    public bool RequireConsent
    {
        get;
        set;
    }

    public bool AllowRememberConsent
    {
        get;
        set;
    }

    public bool AlwaysIncludeUserClaimsInIdToken
    {
        get;
        set;
    }

    public List<ClientGrantType> AllowedGrantTypes
    {
        get;
        set;
    }

    public bool RequirePkce
    {
        get;
        set;
    }

    public bool AllowPlainTextPkce
    {
        get;
        set;
    }

    public bool RequireRequestObject
    {
        get;
        set;
    }

    public bool AllowAccessTokensViaBrowser
    {
        get;
        set;
    }

    public List<ClientRedirectUri> RedirectUris
    {
        get;
        set;
    }

    public List<ClientPostSignOutRedirectUri> PostLogoutRedirectUris
    {
        get;
        set;
    }

    public string FrontChannelLogoutUri
    {
        get;
        set;
    }

    public bool FrontChannelLogoutSessionRequired
    {
        get;
        set;
    }

    public string BackChannelLogoutUri
    {
        get;
        set;
    }

    public bool BackChannelLogoutSessionRequired
    {
        get;
        set;
    }

    public bool AllowOfflineAccess
    {
        get;
        set;
    }

    public List<ClientScope> AllowedScopes
    {
        get;
        set;
    }

    public TimeSpan IdentityTokenLifetime
    {
        get;
        set;
    }

    public string AllowedIdentityTokenSigningAlgorithms
    {
        get;
        set;
    }

    public TimeSpan AccessTokenLifetime
    {
        get;
        set;
    }

    public TimeSpan AuthorizationCodeLifetime
    {
        get;
        set;
    }

    public int? ConsentLifetime
    {
        get;
        set;
    }

    public TimeSpan AbsoluteRefreshTokenLifetime
    {
        get;
        set;
    }

    public TimeSpan SlidingRefreshTokenLifetime
    {
        get;
        set;
    }

    public TokenUsage RefreshTokenUsage
    {
        get;
        set;
    }

    public bool UpdateAccessTokenClaimsOnRefresh
    {
        get;
        set;
    }

    public TokenExpiration RefreshTokenExpiration
    {
        get;
        set;
    }

    public AccessTokenType AccessTokenType
    {
        get;
        set;
    }

    public bool EnableLocalLogin
    {
        get;
        set;
    }

    public List<ClientIdPRestriction> IdentityProviderRestrictions
    {
        get;
        set;
    }

    public bool IncludeJwtId
    {
        get;
        set;
    }

    public List<ClientClaim> Claims
    {
        get;
        set;
    }

    public bool AlwaysSendClientClaims
    {
        get;
        set;
    }

    public string ClientClaimsPrefix
    {
        get;
        set;
    }

    public string PairWiseSubjectSalt
    {
        get;
        set;
    }

    public List<ClientCorsOrigin> AllowedCorsOrigins
    {
        get;
        set;
    }

    public List<ClientProperty> Properties
    {
        get;
        set;
    }

    public int? UserSsoLifetime
    {
        get;
        set;
    }

    public string UserCodeType
    {
        get;
        set;
    }

    public TimeSpan DeviceCodeLifetime
    {
        get;
        set;
    }

    public int? CibaLifetime
    {
        get;
        set;
    }

    public int? PollingInterval
    {
        get;
        set;
    }

    public DateTime Created
    {
        get;
        set;
    }

    public DateTime? Updated
    {
        get;
        set;
    }

    public DateTime? LastAccessed
    {
        get;
        set;
    }

    public bool NonEditable
    {
        get;
        set;
    }

    public Client()
    {
        Enabled = true;
        ProtocolType = "oidc";
        RequireClientSecret = true;
        //RequireConsent = false;
        AllowRememberConsent = true;
        RequirePkce = true;
        FrontChannelLogoutSessionRequired = true;
        BackChannelLogoutSessionRequired = true;
        IdentityTokenLifetime = TimeSpan.FromSeconds(300);
        AccessTokenLifetime = TimeSpan.FromSeconds(3600);
        AuthorizationCodeLifetime = TimeSpan.FromSeconds(300);
        AbsoluteRefreshTokenLifetime = TimeSpan.FromSeconds(2592000);
        SlidingRefreshTokenLifetime = TimeSpan.FromSeconds(1296000);
        RefreshTokenUsage = TokenUsage.OneTimeOnly;
        RefreshTokenExpiration = TokenExpiration.Absolute;
        AccessTokenType = AccessTokenType.Jwt;
        EnableLocalLogin = true;
        ClientClaimsPrefix = "client_";
        DeviceCodeLifetime = TimeSpan.FromSeconds(300);
        //Created = DateTime.UtcNow;
    }
}