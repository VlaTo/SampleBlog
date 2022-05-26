using SampleBlog.IdentityServer.Storage.Models;
using ApiResource = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.ApiResource;
using Client = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.Client;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class EntityFrameworkEntityExtensions
{
    public static IdentityServer.Storage.Models.Client ToModel(this Client source)
    {
        var client = new IdentityServer.Storage.Models.Client
        {
            ClientId = source.ClientId,
            ClientName = source.ClientName,
            ClientUri = source.ClientUri,
            Description = source.Description,
            Enabled = source.Enabled,
            ProtocolType = source.ProtocolType,
            RequireClientSecret = source.RequireClientSecret
        };

        client
            .UseClientSecrets(source.ClientSecrets)
            .UseAllowedGrantTypes(source.AllowedGrantTypes)
            .UseRedirectUris(source.RedirectUris);
        client.UseProperties(source.Properties);
        client.UseClaims(source.Claims);

        return client;
    }

    public static IdentityServer.Storage.Models.ApiResource ToModel(this ApiResource source)
    {
        var resource = new IdentityServer.Storage.Models.ApiResource(source.Name, source.DisplayName)
        {
            Enabled = source.Enabled,
            Scopes = source.Scopes
                .Select(scope => scope.Scope)
                .ToArray(),
            ApiSecrets = source.Secrets
                .Select(secret => new Secret(secret.Value, secret.Description, secret.Expiration))
                .ToArray(),
            Description = source.Description,
            RequireResourceIndicator = source.RequireResourceIndicator,
            ShowInDiscoveryDocument = source.ShowInDiscoveryDocument,
            AllowedAccessTokenSigningAlgorithms = source.AllowedAccessTokenSigningAlgorithms
                .Split(' ')
        };

        resource.UseProperties(source.Properties);
        resource.UseClaims(source.UserClaims);

        return resource;
    }

    public static IdentityResource ToModel(this Entities.IdentityResource source)
    {
        var resource = new IdentityResource(source.Name, source.DisplayName)
        {
            Description = source.Description,
            Emphasize = source.Emphasize,
            Enabled = source.Enabled,
            Required = source.Required,
            ShowInDiscoveryDocument = source.ShowInDiscoveryDocument
        };

        resource.UseProperties(source.Properties);
        resource.UseClaims(source.UserClaims);

        return resource;
    }

    public static ApiScope ToModel(this Entities.ApiScope source)
    {
        var scope = new ApiScope(source.Name, source.DisplayName)
        {
            Description = source.Description,
            Emphasize = source.Emphasize,
            Enabled = source.Enabled,
            Required = source.Required,
            ShowInDiscoveryDocument = source.ShowInDiscoveryDocument
        };

        scope.UseProperties(source.Properties);
        scope.UseClaims(source.UserClaims);

        return scope;
    }

    public static PersistedGrant ToModel(this Entities.PersistedGrant source)
    {
        var grant = new PersistedGrant
        {
            Key = source.Key,
            Description = source.Description,
            Type = source.Type,
            ClientId = source.ClientId,
            SessionId = source.SessionId,
            SubjectId = source.SubjectId
        };

        return grant;
    }

    public static Entities.PersistedGrant ToEntity(this PersistedGrant source)
    {
        var grant = new Entities.PersistedGrant
        {
            Key = source.Key,
            Type = source.Type,
            Description = source.Description,
            ClientId = source.ClientId,
            SessionId = source.SessionId,
            SubjectId = source.SubjectId,
            Data = source.Data,
            CreationTime = source.CreationTime,
            ConsumedTime = source.ConsumedTime,
            Expiration = source.Expiration
        };

        return grant;
    }

    /// <summary>
    /// Updates an entity from a model.
    /// </summary>
    /// <param name="grant"></param>
    /// <param name="source">The entity.</param>
    public static void UpdateEntity(this PersistedGrant grant, Entities.PersistedGrant source)
    {
        grant.Key = source.Key;
        grant.Type = source.Type;
        grant.Description = source.Description;
        grant.ClientId = source.ClientId;
    }
}