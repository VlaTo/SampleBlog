using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using ApiScope = SampleBlog.IdentityServer.Storage.Models.ApiScope;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class ApiScopeExtensions
{
    public static void UseProperties(this ApiScope scope, IList<ApiScopeProperty> properties)
    {
        ;
    }

    public static void UseClaims(this ApiScope scope, IList<ApiScopeClaim> claims)
    {
        ;
    }
}