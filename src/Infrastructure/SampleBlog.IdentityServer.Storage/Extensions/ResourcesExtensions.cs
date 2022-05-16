using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Storage.Extensions;

public static class ResourcesExtensions
{
    internal static Resources FilterEnabled(this Resources? resources)
    {
        if (null == resources)
        {
            return new Resources();
        }

        return new Resources(
            resources.IdentityResources.Where(x => x.Enabled),
            resources.ApiResources.Where(x => x.Enabled),
            resources.ApiScopes.Where(x => x.Enabled),
            resources.OfflineAccess
        );
    }
}