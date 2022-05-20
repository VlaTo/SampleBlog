using System.Collections;
using System.Diagnostics;

namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// Models an OpenID Connect or OAuth2 client
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class Client
{
    // setting grant types should be atomic
    private ICollection<string> allowedGrantTypes = new GrantTypeValidatingHashSet();

    /// <summary>
    /// Specifies if client is enabled (defaults to <c>true</c>)
    /// </summary>
    public bool Enabled
    {
        get;
        set;
    }

    /// <summary>
    /// Unique ID of the client
    /// </summary>
    public string ClientId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the protocol type.
    /// </summary>
    /// <value>
    /// The protocol type.
    /// </value>
    public string ProtocolType
    {
        get;
        set;
    }

    /// <summary>
    /// Client secrets - only relevant for flows that require a secret
    /// </summary>
    public ICollection<Secret> ClientSecrets
    {
        get;
        set;
    }

    /// <summary>
    /// If set to false, no client secret is needed to request tokens at the token endpoint (defaults to <c>true</c>)
    /// </summary>
    public bool RequireClientSecret
    {
        get;
        set;
    }

    /// <summary>
    /// Client display name (used for logging and consent screen)
    /// </summary>
    public string ClientName
    {
        get;
        set;
    }

    /// <summary>
    /// Description of the client.
    /// </summary>
    public string Description
    {
        get;
        set;
    }

    /// <summary>
    /// URI to further information about client (used on consent screen)
    /// </summary>
    public string ClientUri
    {
        get;
        set;
    }

    /// <summary>
    /// URI to client logo (used on consent screen)
    /// </summary>
    public string LogoUri
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether a consent screen is required (defaults to <c>false</c>)
    /// </summary>
    public bool RequireConsent
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether user can choose to store consent decisions (defaults to <c>true</c>)
    /// </summary>
    public bool AllowRememberConsent
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit, Hybrid, ResourceOwner, ClientCredentials).
    /// </summary>
    public ICollection<string> AllowedGrantTypes
    {
        get => allowedGrantTypes;
        set
        {
            ValidateGrantTypes(value);
            allowedGrantTypes = new GrantTypeValidatingHashSet(value);
        }
    }

    /// <summary>
    /// Specifies whether a proof key is required for authorization code based token requests (defaults to <c>true</c>).
    /// </summary>
    public bool RequirePkce
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether a proof key can be sent using plain method (not recommended and defaults to <c>false</c>.)
    /// </summary>
    public bool AllowPlainTextPkce
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether the client must use a request object on authorize requests (defaults to <c>false</c>.)
    /// </summary>
    public bool RequireRequestObject
    {
        get;
        set;
    }

    /// <summary>
    /// Controls whether access tokens are transmitted via the browser for this client (defaults to <c>false</c>).
    /// This can prevent accidental leakage of access tokens when multiple response types are allowed.
    /// </summary>
    /// <value>
    /// <c>true</c> if access tokens can be transmitted via the browser; otherwise, <c>false</c>.
    /// </value>
    public bool AllowAccessTokensViaBrowser
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies allowed URIs to return tokens or authorization codes to
    /// </summary>
    public ICollection<string> RedirectUris
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies allowed URIs to redirect to after logout
    /// </summary>
    public ICollection<string> PostLogoutRedirectUris
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies logout URI at client for HTTP front-channel based logout.
    /// </summary>
    public string FrontChannelLogoutUri
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies if the user's session id should be sent to the FrontChannelLogoutUri. Defaults to <c>true</c>.
    /// </summary>
    public bool FrontChannelLogoutSessionRequired
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies logout URI at client for HTTP back-channel based logout.
    /// </summary>
    public string BackChannelLogoutUri
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies if the user's session id should be sent to the BackChannelLogoutUri. Defaults to <c>true</c>.
    /// </summary>
    public bool BackChannelLogoutSessionRequired
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether [allow offline access]. Defaults to <c>false</c>.
    /// </summary>
    public bool AllowOfflineAccess
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies the api scopes that the client is allowed to request. If empty, the client can't access any scope
    /// </summary>
    public ICollection<string> AllowedScopes
    {
        get;
        set;
    }

    /// <summary>
    /// When requesting both an id token and access token, should the user claims always be added to the id token instead of requiring the client to use the userinfo endpoint.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool AlwaysIncludeUserClaimsInIdToken
    {
        get;
        set;
    }

    /// <summary>
    /// Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
    /// </summary>
    public TimeSpan IdentityTokenLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Signing algorithm for identity token. If empty, will use the server default signing algorithm.
    /// </summary>
    public ICollection<string> AllowedIdentityTokenSigningAlgorithms
    {
        get;
        set;
    }

    /// <summary>
    /// Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
    /// </summary>
    public TimeSpan AccessTokenLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
    /// </summary>
    public TimeSpan AuthorizationCodeLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days
    /// </summary>
    public TimeSpan AbsoluteRefreshTokenLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days
    /// </summary>
    public TimeSpan SlidingRefreshTokenLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Lifetime of a user consent in seconds. Defaults to null (no expiration)
    /// </summary>
    public TimeSpan? ConsentLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// ReUse: the refresh token handle will stay the same when refreshing tokens
    /// OneTime: the refresh token handle will be updated when refreshing tokens
    /// </summary>
    public TokenUsage RefreshTokenUsage
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request.
    /// Defaults to <c>false</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> if the token should be updated; otherwise, <c>false</c>.
    /// </value>
    public bool UpdateAccessTokenClaimsOnRefresh
    {
        get;
        set;
    }

    /// <summary>
    /// Absolute: the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
    /// Sliding: when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed AbsoluteRefreshTokenLifetime.
    /// </summary>        
    public TokenExpiration RefreshTokenExpiration
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether the access token is a reference token or a self contained JWT token (defaults to Jwt).
    /// </summary>
    public AccessTokenType AccessTokenType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the local login is allowed for this client. Defaults to <c>true</c>.
    /// </summary>
    /// <value>
    ///   <c>true</c> if local logins are enabled; otherwise, <c>false</c>.
    /// </value>
    public bool EnableLocalLogin
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies which external IdPs can be used with this client (if list is empty all IdPs are allowed). Defaults to empty.
    /// </summary>
    public ICollection<string> IdentityProviderRestrictions
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether JWT access tokens should include an identifier. Defaults to <c>true</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> to add an id; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeJwtId
    {
        get;
        set;
    }

    /// <summary>
    /// Allows settings claims for the client (will be included in the access token).
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    public ICollection<ClientClaim> Claims
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether client claims should be always included in the access tokens - or only for client credentials flow.
    /// Defaults to <c>false</c>
    /// </summary>
    /// <value>
    /// <c>true</c> if claims should always be sent; otherwise, <c>false</c>.
    /// </value>
    public bool AlwaysSendClientClaims
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value to prefix it on client claim types. Defaults to <c>client_</c>.
    /// </summary>
    /// <value>
    /// Any non empty string if claims should be prefixed with the value; otherwise, <c>null</c>.
    /// </value>
    public string ClientClaimsPrefix
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a salt value used in pair-wise subjectId generation for users of this client.
    /// </summary>
    public string PairWiseSubjectSalt
    {
        get;
        set;
    }

    /// <summary>
    /// The maximum duration (in seconds) since the last time the user authenticated.
    /// </summary>
    public int? UserSsoLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type of the device flow user code.
    /// </summary>
    /// <value>
    /// The type of the device flow user code.
    /// </value>
    public string UserCodeType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the device code lifetime.
    /// </summary>
    /// <value>
    /// The device code lifetime.
    /// </value>
    public TimeSpan DeviceCodeLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the backchannel authentication request lifetime in seconds.
    /// </summary>
    public int? CibaLifetime
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the backchannel polling interval in seconds.
    /// </summary>
    public int? PollingInterval
    {
        get;
        set;
    }


    /// <summary>
    /// When enabled, the client's token lifetimes (e.g. refresh tokens) will be tied to the user's session lifetime.
    /// This means when the user logs out, any revokable tokens will be removed.
    /// If using server-side sessions, expired sessions will also remove any revokable tokens, and backchannel logout will be triggered.
    /// This client's setting overrides the global CoordinateTokensWithUserSession configuration setting.
    /// </summary>
    public bool? CoordinateLifetimeWithUserSession
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets the allowed CORS origins for JavaScript clients.
    /// </summary>
    /// <value>
    /// The allowed CORS origins.
    /// </value>
    public ICollection<string> AllowedCorsOrigins
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the custom properties for the client.
    /// </summary>
    /// <value>
    /// The properties.
    /// </value>
    public IDictionary<string, string> Properties
    {
        get;
        set;
    }

    public Client()
    {
        Enabled = true;
        ProtocolType = IdentityServerConstants.ProtocolTypes.OpenIdConnect;
        ClientSecrets = new HashSet<Secret>();
        RequireClientSecret = true;
        //RequireConsent = false;
        AllowRememberConsent = true;
        RequirePkce = true;
        //AllowPlainTextPkce = false;
        //RequireRequestObject = false;
        //AllowAccessTokensViaBrowser = false;
        RedirectUris = new HashSet<string>();
        PostLogoutRedirectUris = new HashSet<string>();
        FrontChannelLogoutSessionRequired = true;
        BackChannelLogoutSessionRequired = true;
        AllowedScopes = new HashSet<string>();
        //AlwaysIncludeUserClaimsInIdToken = false;
        IdentityTokenLifetime = TimeSpan.FromMinutes(5.0d);
        AccessTokenLifetime = TimeSpan.FromHours(1.0d);
        AuthorizationCodeLifetime = TimeSpan.FromMinutes(5.0d);
        AbsoluteRefreshTokenLifetime = TimeSpan.FromDays(30.0d);
        SlidingRefreshTokenLifetime = TimeSpan.FromDays(15.0d);
        AllowedIdentityTokenSigningAlgorithms = new HashSet<string>();
        ConsentLifetime = null;
        RefreshTokenUsage = TokenUsage.OneTimeOnly;
        //UpdateAccessTokenClaimsOnRefresh = false;
        RefreshTokenExpiration = TokenExpiration.Absolute;
        AccessTokenType = AccessTokenType.Jwt;
        EnableLocalLogin = true;
        IdentityProviderRestrictions = new HashSet<string>();
        IncludeJwtId = true;
        Claims = new HashSet<ClientClaim>();
        //AlwaysSendClientClaims = false;
        ClientClaimsPrefix = "client_";
        DeviceCodeLifetime = TimeSpan.FromMinutes(5.0d);
        AllowedCorsOrigins = new HashSet<string>();
        Properties = new Dictionary<string, string>();
    }

    /// <summary>
    /// Validates the grant types.
    /// </summary>
    /// <param name="grantTypes">The grant types.</param>
    /// <exception cref="System.InvalidOperationException">
    /// Grant types list is empty
    /// or
    /// Grant types cannot contain spaces
    /// or
    /// Grant types list contains duplicate values
    /// </exception>
    public static void ValidateGrantTypes(IEnumerable<string>? grantTypes)
    {
        if (null == grantTypes)
        {
            throw new ArgumentNullException(nameof(grantTypes));
        }

        var grantTypesArray = grantTypes.ToArray();

        // spaces are not allowed in grant types
        foreach (var type in grantTypesArray)
        {
            if (type.Contains(' '))
            {
                throw new InvalidOperationException("Grant types cannot contain spaces");
            }
        }

        // single grant type, seems to be fine
        if (1 == grantTypesArray.Length)
        {
            return;
        }

        // don't allow duplicate grant types
        if (grantTypesArray.Distinct().Count() != grantTypesArray.Length)
        {
            throw new InvalidOperationException("Grant types list contains duplicate values");
        }

        // would allow response_type downgrade attack from code to token
        DisallowGrantTypeCombination(grantTypesArray, GrantType.Implicit, GrantType.AuthorizationCode);
        DisallowGrantTypeCombination(grantTypesArray, GrantType.Implicit, GrantType.Hybrid);

        DisallowGrantTypeCombination(grantTypesArray, GrantType.AuthorizationCode, GrantType.Hybrid);
    }

    private static void DisallowGrantTypeCombination(string[] grantTypes, string value1, string value2)
    {
        var exists = Array.TrueForAll(
            new[] { value1, value2 },
            value => -1 < Array.IndexOf(grantTypes, value)
        );

        if (exists)
        {
            throw new InvalidOperationException($"Grant types list cannot contain both {value1} and {value2}");
        }
    }

    #region GrantTypeValidatingHashSet

    internal sealed class GrantTypeValidatingHashSet : ICollection<string>
    {
        private readonly ICollection<string> values;

        public int Count => values.Count;

        public bool IsReadOnly => values.IsReadOnly;

        public GrantTypeValidatingHashSet()
        {
            values = new HashSet<string>();
        }

        public GrantTypeValidatingHashSet(IEnumerable<string> values)
        {
            this.values = new HashSet<string>(values);
        }

        public void Add(string item)
        {
            ValidateGrantTypes(CloneWith(item));
            values.Add(item);
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(string item)
        {
            return values.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public bool Remove(string item)
        {
            return values.Remove(item);
        }

        private ICollection<string> CloneWith(params string[] array)
        {
            var clone = Clone();

            foreach (var item in array)
            {
                clone.Add(item);
            }
            
            return clone;
        }

        private ICollection<string> Clone() => new HashSet<string>(this);

        IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
    }

    #endregion

    #region DebuggerDisplay

    private string DebuggerDisplay => ClientId ?? $"{{{typeof(Client)}}}";

    #endregion
}