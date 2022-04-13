using SampleBlog.Identity.Authorization.Configuration;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.Identity.Authorization.Options;

/// <summary>
/// A builder for API resources
/// </summary>
public class ApiResourceBuilder
{
    private readonly ApiResource apiResource;
    private bool built;

    /// <summary>
    /// Creates a new builder for an externally registered API.
    /// </summary>
    /// <param name="name">The name of the API.</param>
    /// <returns>An <see cref="ApiResourceBuilder"/>.</returns>
    public static ApiResourceBuilder ApiResource(string name)
    {
        var apiResource = new ApiResource(name);

        return new ApiResourceBuilder(apiResource)
            .WithApplicationProfile(ApplicationProfiles.API)
            .WithScopes(name);
    }

    /// <summary>
    /// Creates a new builder for an API that coexists with an authorization server.
    /// </summary>
    /// <param name="name">The name of the API.</param>
    /// <returns>An <see cref="ApiResourceBuilder"/>.</returns>
    public static ApiResourceBuilder IdentityServerJwt(string name)
    {
        var apiResource = new ApiResource(name);

        return new ApiResourceBuilder(apiResource)
            .WithApplicationProfile(ApplicationProfiles.IdentityServerJwt);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApiResourceBuilder"/>.
    /// </summary>
    /*public ApiResourceBuilder()
        : this(new ApiResource())
    {
    }*/

    /// <summary>
    /// Initializes a new instance of <see cref="ApiResourceBuilder"/>.
    /// </summary>
    /// <param name="apiResource">A preconfigured resource.</param>
    public ApiResourceBuilder(ApiResource apiResource)
    {
        this.apiResource = apiResource;
    }

    /// <summary>
    /// Sets the application profile for the resource.
    /// </summary>
    /// <param name="profile">The the profile for the application from <see cref="ApplicationProfiles"/>.</param>
    /// <returns>The <see cref="ApiResourceBuilder"/>.</returns>
    public ApiResourceBuilder WithApplicationProfile(string profile)
    {
        apiResource.Properties.Add(ApplicationProfilesPropertyNames.Profile, profile);
        return this;
    }

    /// <summary>
    /// Adds additional scopes to the API resource.
    /// </summary>
    /// <param name="resourceScopes">The list of scopes.</param>
    /// <returns>The <see cref="ApiResourceBuilder"/>.</returns>
    public ApiResourceBuilder WithScopes(params string[] resourceScopes)
    {
        foreach (var scope in resourceScopes)
        {
            if (apiResource.Scopes.Any(s => s == scope))
            {
                continue;
            }

            apiResource.Scopes.Add(scope);
        }

        return this;
    }

    /// <summary>
    /// Replaces the scopes defined for the application with a new set of scopes.
    /// </summary>
    /// <param name="resourceScopes">The list of scopes.</param>
    /// <returns>The <see cref="ApiResourceBuilder"/>.</returns>
    public ApiResourceBuilder ReplaceScopes(params string[] resourceScopes)
    {
        apiResource.Scopes.Clear();

        return WithScopes(resourceScopes);
    }

    /// <summary>
    /// Configures the API resource to allow all clients to access it.
    /// </summary>
    /// <returns>The <see cref="ApiResourceBuilder"/>.</returns>
    public ApiResourceBuilder AllowAllClients()
    {
        apiResource.Properties[ApplicationProfilesPropertyNames.Clients] = ApplicationProfilesPropertyValues.AllowAllApplications;
        return this;
    }

    /// <summary>
    /// Builds the API resource.
    /// </summary>
    /// <returns>The built <see cref="Duende.IdentityServer.Models.ApiResource"/>.</returns>
    public ApiResource Build()
    {
        if (built)
        {
            throw new InvalidOperationException("ApiResource already built.");
        }

        built = true;

        return apiResource;
    }

    internal ApiResourceBuilder WithAllowedClients(string clientList)
    {
        apiResource.Properties[ApplicationProfilesPropertyNames.Clients] = clientList;
        return this;
    }

    internal ApiResourceBuilder FromConfiguration()
    {
        apiResource.Properties[ApplicationProfilesPropertyNames.Source] = ApplicationProfilesPropertyValues.Configuration;
        return this;
    }
}