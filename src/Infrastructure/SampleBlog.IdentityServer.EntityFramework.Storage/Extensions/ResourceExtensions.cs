using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.Storage.Models;
using ApiScope = SampleBlog.IdentityServer.Storage.Models.ApiScope;
using ClientClaim = SampleBlog.IdentityServer.EntityFramework.Storage.Entities.ClientClaim;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class ResourceExtensions
{
    public static void UseProperties(this IdentityServer.Storage.Models.Client client, IDictionary<string, string> properties)
    {
        ;
    }

    public static void UseProperties(this Resource resource, IList<ApiResourceProperty> properties)
    {
        ;
    }

    public static void UseProperties(this Resource resource, IList<IdentityResourceProperty> properties)
    {
        ;
    }
    public static void UseProperties(this ApiScope scope, IList<ApiScopeProperty> properties)
    {
        ;
    }

    public static void UseClaims(this IdentityServer.Storage.Models.Client client, IList<ClientClaim> claims)
    {
        ;
    }

    public static void UseClaims(this Resource resource, IList<ApiResourceClaim> claims)
    {
        ;
    }

    public static void UseClaims(this ApiScope scope, IList<ApiScopeClaim> claims)
    {
        ;
    }

    public static void UseClaims(this Resource resource, IList<IdentityResourceClaim> claims)
    {
        ;
    }
}