using SampleBlog.Identity.Authorization.Configuration;
using SampleBlog.IdentityServer;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.Identity.Authorization.Options;

/// <summary>
/// A builder for identity resources
/// </summary>
public class IdentityResourceBuilder
{
    private readonly IdentityResource resource;
    private bool built;

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityResourceBuilder"/>.
    /// </summary>
    /*public IdentityResourceBuilder()
        : this(new IdentityResource())
    {
    }*/

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityResourceBuilder"/>.
    /// </summary>
    /// <param name="resource">A preconfigured resource.</param>
    public IdentityResourceBuilder(IdentityResource resource)
    {
        this.resource = resource;
    }

    /// <summary>
    /// Creates an openid resource.
    /// </summary>
    public static IdentityResourceBuilder OpenId() =>
        IdentityResource(IdentityServerConstants.StandardScopes.OpenId);

    /// <summary>
    /// Creates a profile resource.
    /// </summary>
    public static IdentityResourceBuilder Profile() =>
        IdentityResource(IdentityServerConstants.StandardScopes.Profile);

    /// <summary>
    /// Builds the API resource.
    /// </summary>
    /// <returns>The built <see cref="Duende.IdentityServer.Models.IdentityResource"/>.</returns>
    public IdentityResource Build()
    {
        if (built)
        {
            throw new InvalidOperationException("IdentityResource already built.");
        }

        built = true;
        return resource;
    }

    /// <summary>
    /// Configures the API resource to allow all clients to access it.
    /// </summary>
    /// <returns>The <see cref="IdentityResourceBuilder"/>.</returns>
    public IdentityResourceBuilder AllowAllClients()
    {
        resource.Properties[ApplicationProfilesPropertyNames.Clients] = ApplicationProfilesPropertyValues.AllowAllApplications;
        return this;
    }

    internal IdentityResourceBuilder WithAllowedClients(string clientList)
    {
        resource.Properties[ApplicationProfilesPropertyNames.Clients] = clientList;
        return this;
    }
    
    internal IdentityResourceBuilder FromConfiguration()
    {
        resource.Properties[ApplicationProfilesPropertyNames.Source] = ApplicationProfilesPropertyValues.Configuration;
        return this;
    }

    internal IdentityResourceBuilder FromDefault()
    {
        resource.Properties[ApplicationProfilesPropertyNames.Source] = ApplicationProfilesPropertyValues.Default;
        return this;
    }

    internal static IdentityResourceBuilder IdentityResource(string name)
    {
        var identityResource = GetResource(name);
        return new IdentityResourceBuilder(identityResource);
    }

    private static IdentityResource GetResource(string name)
    {
        switch (name)
        {
            case IdentityServerConstants.StandardScopes.OpenId:
            {
                return new IdentityResources.OpenId();
            }

            case IdentityServerConstants.StandardScopes.Profile:
            {
                return new IdentityResources.Profile();
            }

            /*case IdentityServerConstants.StandardScopes.Address:
                return new IdentityResources.Address();
            case IdentityServerConstants.StandardScopes.Email:
                return new IdentityResources.Email();
            case IdentityServerConstants.StandardScopes.Phone:
                return new IdentityResources.Phone();*/

            default:
            {
                throw new InvalidOperationException("Invalid identity resource type.");
            }
        }
    }
}