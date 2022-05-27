using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class ResourceExtensions
{
    public static void UseProperties(this Resource resource, IList<ApiResourceProperty> properties)
    {
        resource.Properties = new Dictionary<string, string>();

        for (var index = 0; index < properties.Count; index++)
        {
            var property = properties[index];
            resource.Properties.Add(property.Key, property.Value);
        }
    }

    public static void UseProperties(this Resource resource, IList<IdentityResourceProperty> properties)
    {
        resource.Properties = new Dictionary<string, string>();

        for (var index = 0; index < properties.Count; index++)
        {
            var property = properties[index];
            resource.Properties.Add(property.Key, property.Value);
        }
    }

    public static void UseClaims(this Resource resource, IList<ApiResourceClaim> claims)
    {
        resource.UserClaims = new HashSet<string>(claims.Count);

        for (var index = 0; index < claims.Count; index++)
        {
            resource.UserClaims.Add(claims[index].Type);
        }
    }

    public static void UseClaims(this Resource resource, IList<IdentityResourceClaim> claims)
    {
        resource.UserClaims = new HashSet<string>(claims.Count);

        for (var index = 0; index < claims.Count; index++)
        {
            resource.UserClaims.Add(claims[index].Type);
        }
    }
}