using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Extensions;

public static class ResourceValidationResultExtensions
{
    /// <summary>
    /// Returns the collection of scope values that are required.
    /// </summary>
    /// <param name="resourceValidationResult"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetRequiredScopeValues(this ResourceValidationResult resourceValidationResult)
    {
        var names = resourceValidationResult.Resources.IdentityResources
            .Where(x => x.Required)
            .Select(x => x.Name)
            .ToList();

        names.AddRange(
            resourceValidationResult.Resources.ApiScopes.Where(x => x.Required).Select(x => x.Name)
        );

        var values = resourceValidationResult.ParsedScopes
            .Where(x => names.Contains(x.ParsedName))
            .Select(x => x.RawValue);

        return values;
    }
}