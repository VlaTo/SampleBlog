using Microsoft.EntityFrameworkCore;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Options;

/// <summary>
/// Options for configuring the configuration context.
/// </summary>
public class ConfigurationStoreOptions
{
    /// <summary>
    /// Callback to configure the EF DbContext.
    /// </summary>
    /// <value>
    /// The configure database context.
    /// </value>
    public Action<DbContextOptionsBuilder>? ConfigureDbContext
    {
        get;
        set;
    }

    /// <summary>
    /// Callback in DI resolve the EF DbContextOptions. If set, ConfigureDbContext will not be used.
    /// </summary>
    /// <value>
    /// The configure database context.
    /// </value>
    public Action<IServiceProvider, DbContextOptionsBuilder>? ResolveDbContextOptions
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the default schema.
    /// </summary>
    /// <value>
    /// The default schema.
    /// </value>
    public string? DefaultSchema
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identity resource table configuration.
    /// </summary>
    /// <value>
    /// The identity resource.
    /// </value>
    public TableConfiguration IdentityResource
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identity claim table configuration.
    /// </summary>
    /// <value>
    /// The identity claim.
    /// </value>
    public TableConfiguration IdentityResourceClaim
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identity resource property table configuration.
    /// </summary>
    /// <value>
    /// The client property.
    /// </value>
    public TableConfiguration IdentityResourceProperty
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the API resource table configuration.
    /// </summary>
    /// <value>
    /// The API resource.
    /// </value>
    public TableConfiguration ApiResource
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the API secret table configuration.
    /// </summary>
    /// <value>
    /// The API secret.
    /// </value>
    public TableConfiguration ApiResourceSecret
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the API scope table configuration.
    /// </summary>
    /// <value>
    /// The API scope.
    /// </value>
    public TableConfiguration ApiResourceScope
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the API claim table configuration.
    /// </summary>
    /// <value>
    /// The API claim.
    /// </value>
    public TableConfiguration ApiResourceClaim
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the API resource property table configuration.
    /// </summary>
    /// <value>
    /// The client property.
    /// </value>
    public TableConfiguration ApiResourceProperty
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client table configuration.
    /// </summary>
    /// <value>
    /// The client.
    /// </value>
    public TableConfiguration Client
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the type of the client grant table configuration.
    /// </summary>
    /// <value>
    /// The type of the client grant.
    /// </value>
    public TableConfiguration ClientGrantType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client redirect URI table configuration.
    /// </summary>
    /// <value>
    /// The client redirect URI.
    /// </value>
    public TableConfiguration ClientRedirectUri
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client post logout redirect URI table configuration.
    /// </summary>
    /// <value>
    /// The client post logout redirect URI.
    /// </value>
    public TableConfiguration ClientPostLogoutRedirectUri
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client scopes table configuration.
    /// </summary>
    /// <value>
    /// The client scopes.
    /// </value>
    public TableConfiguration ClientScopes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client secret table configuration.
    /// </summary>
    /// <value>
    /// The client secret.
    /// </value>
    public TableConfiguration ClientSecret
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client claim table configuration.
    /// </summary>
    /// <value>
    /// The client claim.
    /// </value>
    public TableConfiguration ClientClaim
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client IdP restriction table configuration.
    /// </summary>
    /// <value>
    /// The client IdP restriction.
    /// </value>
    public TableConfiguration ClientIdPRestriction
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client cors origin table configuration.
    /// </summary>
    /// <value>
    /// The client cors origin.
    /// </value>
    public TableConfiguration ClientCorsOrigin
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client property table configuration.
    /// </summary>
    /// <value>
    /// The client property.
    /// </value>
    public TableConfiguration ClientProperty
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the scope table configuration.
    /// </summary>
    /// <value>
    /// The API resource.
    /// </value>
    public TableConfiguration ApiScope
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the scope claim table configuration.
    /// </summary>
    /// <value>
    /// The API scope claim.
    /// </value>
    public TableConfiguration ApiScopeClaim
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the API resource property table configuration.
    /// </summary>
    /// <value>
    /// The client property.
    /// </value>
    public TableConfiguration ApiScopeProperty
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identity providers table configuration.
    /// </summary>
    public TableConfiguration IdentityProvider
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or set if EF DbContext pooling is enabled.
    /// </summary>
    public bool EnablePooling
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or set the pool size to use when DbContext pooling is enabled. If not set, the EF default is used.
    /// </summary>
    public int? PoolSize
    {
        get;
        set;
    }

    public ConfigurationStoreOptions()
    {
        DefaultSchema = null;
        IdentityResource = new TableConfiguration("IdentityResources", Database.Schemas.Identity);
        IdentityResourceClaim = new TableConfiguration("IdentityResourceClaims", Database.Schemas.Identity);
        IdentityResourceProperty = new TableConfiguration("IdentityResourceProperties", Database.Schemas.Identity);
        ApiResource = new TableConfiguration("ApiResources", Database.Schemas.Identity);
        ApiResourceSecret = new TableConfiguration("ApiResourceSecrets", Database.Schemas.Identity);
        ApiResourceScope = new TableConfiguration("ApiResourceScopes", Database.Schemas.Identity);
        ApiResourceClaim = new TableConfiguration("ApiResourceClaims", Database.Schemas.Identity);
        ApiResourceProperty = new TableConfiguration("ApiResourceProperties", Database.Schemas.Identity);
        Client = new TableConfiguration("Clients", Database.Schemas.Identity);
        ClientGrantType = new TableConfiguration("ClientGrantTypes", Database.Schemas.Identity);
        ClientRedirectUri = new TableConfiguration("ClientRedirectUris", Database.Schemas.Identity);
        ClientPostLogoutRedirectUri = new TableConfiguration("ClientPostLogoutRedirectUris", Database.Schemas.Identity);
        ClientScopes = new TableConfiguration("ClientScopes", Database.Schemas.Identity);
        ClientSecret = new TableConfiguration("ClientSecrets", Database.Schemas.Identity);
        ClientClaim = new TableConfiguration("ClientClaims", Database.Schemas.Identity);
        ClientIdPRestriction = new TableConfiguration("ClientIdPRestrictions", Database.Schemas.Identity);
        ClientCorsOrigin = new TableConfiguration("ClientCorsOrigins", Database.Schemas.Identity);
        ClientProperty = new TableConfiguration("ClientProperties", Database.Schemas.Identity);
        ApiScope = new TableConfiguration("ApiScopes", Database.Schemas.Identity);
        ApiScopeClaim = new TableConfiguration("ApiScopeClaims", Database.Schemas.Identity);
        ApiScopeProperty = new TableConfiguration("ApiScopeProperties", Database.Schemas.Identity);
        IdentityProvider = new TableConfiguration("IdentityProviders", Database.Schemas.Identity);
        EnablePooling = false;
    }
}