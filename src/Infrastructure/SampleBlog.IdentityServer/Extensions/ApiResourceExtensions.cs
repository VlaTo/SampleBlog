using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Extensions;

public static class ApiResourceExtensions
{
    internal static ICollection<string> FindMatchingSigningAlgorithms(this IEnumerable<ApiResource> apiResources)
    {
        var apis = apiResources.ToArray();

        if (0 < apis.Length)
        {
            // only one API resource request, forward the allowed signing algorithms (if any)
            if (1 == apis.Length)
            {
                return apis[0].AllowedAccessTokenSigningAlgorithms;
            }

            var allAlgorithms = apis
                .Where(r => r.AllowedAccessTokenSigningAlgorithms.Any())
                .Select(r => r.AllowedAccessTokenSigningAlgorithms)
                .ToArray();

            // resources need to agree on allowed signing algorithms
            if (0 < allAlgorithms.Length)
            {
                var allowedAlgorithms = IntersectLists(allAlgorithms);

                if (allowedAlgorithms.Any())
                {
                    return allowedAlgorithms.ToHashSet();
                }

                throw new InvalidOperationException("Signing algorithms requirements for requested resources are not compatible.");
            }
        }

        return new List<string>();
    }

    private static IEnumerable<T> IntersectLists<T>(IEnumerable<IEnumerable<T>> lists)
    {
        return lists.Aggregate((l1, l2) => l1.Intersect(l2));
    }
}