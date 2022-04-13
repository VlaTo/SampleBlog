using Microsoft.IdentityModel.Tokens;

namespace SampleBlog.Identity.Authorization.Options;

/// <summary>
/// Options for API authorization.
/// </summary>
public class ApiAuthorizationOptions
{
    /// <summary>
    /// Gets or sets the <see cref="IdentityResources"/>.
    /// </summary>
    public IdentityResourceCollection IdentityResources
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the <see cref="ApiResources"/>.
    /// </summary>
    public ApiResourceCollection ApiResources
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the <see cref="ApiScopes"/>.
    /// </summary>
    public ApiScopeCollection ApiScopes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the <see cref="Clients"/>.
    /// </summary>
    public ClientCollection Clients
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the <see cref="SigningCredentials"/> to use for signing tokens.
    /// </summary>
    public SigningCredentials SigningCredential
    {
        get;
        set;
    }

    public ApiAuthorizationOptions()
    {
        IdentityResources = new IdentityResourceCollection
        {
            IdentityResourceBuilder.OpenId()
                .AllowAllClients()
                .FromDefault()
                .Build(),
            IdentityResourceBuilder.Profile()
                .AllowAllClients()
                .FromDefault()
                .Build()
        };
        ApiResources = new ApiResourceCollection();
        Clients = new ClientCollection();
        ApiScopes = new ApiScopeCollection();
    }
}