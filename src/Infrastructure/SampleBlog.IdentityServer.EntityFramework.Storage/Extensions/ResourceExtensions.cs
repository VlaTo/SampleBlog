using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class ResourceExtensions
{
    public static void UseProperties(this Resource resource, IList<ApiResourceProperty> properties)
    {
        ;
    }

    public static void UseProperties(this Resource resource, IList<IdentityResourceProperty> properties)
    {
        ;
    }

    public static void UseClaims(this Resource resource, IList<ApiResourceClaim> claims)
    {
        ;
    }

    public static void UseClaims(this Resource resource, IList<IdentityResourceClaim> claims)
    {
        ;
    }
}