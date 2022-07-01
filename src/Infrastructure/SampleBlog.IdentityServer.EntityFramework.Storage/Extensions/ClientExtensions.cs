using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.Storage.Models;
using Client = SampleBlog.IdentityServer.Storage.Models.Client;
using ClientClaim = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.ClientClaim;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class ClientExtensions
{
    public static void UseProperties(this Client client, IList<ClientProperty> properties)
    {
        ;
    }

    public static void UseProperties(this Client client, IDictionary<string, string> properties)
    {
        ;
    }

    public static void UseClaims(this Client client, IList<ClientClaim> claims)
    {
        ;
    }

    public static Client UseAllowedGrantTypes(this Client client, IList<ClientGrantType> grantTypes)
    {
        client.AllowedGrantTypes = new HashSet<string>(grantTypes.Count);

        for (var index = 0; index < grantTypes.Count; index++)
        {
            client.AllowedGrantTypes.Add(grantTypes[index].GrantType);
        }

        return client;
    }

    public static Client UseClientSecrets(this Client client, IList<ClientSecret> secrets)
    {
        var clientSecrets = new IdentityServer.Storage.Models.Secret[secrets.Count];

        for (var index = 0; index < secrets.Count; index++)
        {
            var secret = secrets[index];
            clientSecrets[index] = new IdentityServer.Storage.Models.Secret(secret.Value, secret.Description, secret.Expiration);
        }

        client.ClientSecrets = clientSecrets;

        return client;
    }

    public static Client UseRedirectUris(this Client client, IList<ClientRedirectUri> redirectUris)
    {
        client.RedirectUris = new HashSet<string>(redirectUris.Count);

        for (var index = 0; index < redirectUris.Count; index++)
        {
            var source = redirectUris[index];
            client.RedirectUris.Add(source.RedirectUri);
        }

        return client;
    }

    public static Client UseAllowedScopes(this Client client, IList<ClientScope> scopes)
    {
        client.AllowedScopes = new HashSet<string>(scopes.Count);

        for (var index = 0; index < scopes.Count; index++)
        {
            var scope = scopes[index];
            client.AllowedScopes.Add(scope.Scope);
        }

        return client;
    }

    public static Client UseAllowedIdentityTokenSigningAlgorithms(this Client client, string signingAlgorithms)
    {
        client.AllowedIdentityTokenSigningAlgorithms = signingAlgorithms.Split(
            new[] { ' ', ',' },
            StringSplitOptions.RemoveEmptyEntries
        );

        return client;
    }
}