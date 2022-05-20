using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Models;

/// <summary>
/// Result of validation of requested scopes and resource indicators.
/// </summary>
public class ResourceValidationResult
{
    /// <summary>
    /// Indicates if the result was successful.
    /// </summary>
    public bool Succeeded => ParsedScopes.Any() && !InvalidScopes.Any() && !InvalidResourceIndicators.Any();

    /// <summary>
    /// The resources of the result.
    /// </summary>
    public Resources Resources
    {
        get;
        set;
    }

    /// <summary>
    /// The parsed scopes represented by the result.
    /// </summary>
    public IList<ParsedScopeValue> ParsedScopes
    {
        get;
        set;
    }

    /// <summary>
    /// The original (raw) scope values represented by the validated result.
    /// </summary>
    public IEnumerable<string> RawScopeValues => ParsedScopes.Select(x => x.RawValue);

    /// <summary>
    /// The requested resource indicators that are invalid.
    /// </summary>
    public ICollection<string> InvalidResourceIndicators
    {
        get;
        set;
    }

    /// <summary>
    /// The requested scopes that are invalid.
    /// </summary>
    public ICollection<string> InvalidScopes
    {
        get;
        set;
    }

    /// <summary>
    /// Ctor
    /// </summary>
    public ResourceValidationResult()
        : this(new Resources(), new HashSet<ParsedScopeValue>())
    {
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="resources"></param>
    public ResourceValidationResult(Resources resources)
        : this(resources, resources.ToScopeNames().Select(x => new ParsedScopeValue(x)))
    {
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="parsedScopeValues"></param>
    public ResourceValidationResult(Resources resources, IEnumerable<ParsedScopeValue> parsedScopeValues)
    {
        Resources = resources;
        ParsedScopes = parsedScopeValues.ToList();
        InvalidResourceIndicators = new HashSet<string>();
        InvalidScopes = new HashSet<string>();
    }

    /// <summary>
    /// Returns new result filted by the scope values.
    /// </summary>
    /// <param name="scopeValues"></param>
    /// <returns></returns>
    public ResourceValidationResult Filter(IEnumerable<string>? scopeValues)
    {
        var scopeNames = (scopeValues ?? Enumerable.Empty<string>()).ToArray();
        var offline = scopeNames.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

        var parsedScopesToKeep = ParsedScopes
            .Where(x => scopeNames.Contains(x.RawValue))
            .ToArray();
        var parsedScopeNamesToKeep = parsedScopesToKeep
            .Select(x => x.ParsedName)
            .ToArray();

        var identityToKeep = Resources.IdentityResources
            .Where(resource => parsedScopeNamesToKeep.Contains(resource.Name));
        var apiScopesToKeep = Resources.ApiScopes
            .Where(scope => parsedScopeNamesToKeep.Contains(scope.Name))
            .ToArray();

        var apiScopesNamesToKeep = apiScopesToKeep
            .Select(scope => scope.Name)
            .ToArray();
        var apiResourcesToKeep = Resources.ApiResources
            .Where(
                resource => resource.Scopes.Any(scope => apiScopesNamesToKeep.Contains(scope))
            );

        var resources = Resources.Create(identityToKeep, apiResourcesToKeep, apiScopesToKeep, offline);

        return new ResourceValidationResult(resources, parsedScopesToKeep);
    }

    /// <summary>
    /// Filters the result by the resource indicator for issuing access tokens.
    /// </summary>
    public ResourceValidationResult FilterByResourceIndicator(string resourceIndicator)
    {
        // filter ApiResources to only the ones allowed by the resource indicator requested
        var apiResourcesToKeep = (String.IsNullOrWhiteSpace(resourceIndicator)
            ? Resources.ApiResources.Where(resource => false == resource.RequireResourceIndicator)
            : Resources.ApiResources.Where(resource => String.Equals(resource.Name, resourceIndicator))).ToArray();

        var apiScopesToKeep = Resources.ApiScopes.AsEnumerable();
        var parsedScopesToKeep = ParsedScopes;

        if (false == String.IsNullOrWhiteSpace(resourceIndicator))
        {
            // filter ApiScopes to only the ones allowed by the ApiResource requested
            var scopeNamesToKeep = apiResourcesToKeep
                .SelectMany(resource => resource.Scopes)
                .Select(resource => resource)
                .ToArray();

            apiScopesToKeep = Resources.ApiScopes
                .Where(x => scopeNamesToKeep.Contains(x.Name))
                .ToArray();

            // filter ParsedScopes to those matching the apiScopesToKeep
            var parsedScopesToKeepList = ParsedScopes
                .Where(x => scopeNamesToKeep.Contains(x.ParsedName))
                .ToList();

            if (Resources.OfflineAccess)
            {
                parsedScopesToKeepList.Add(new ParsedScopeValue(IdentityServerConstants.StandardScopes.OfflineAccess));
            }

            parsedScopesToKeep = parsedScopesToKeepList;
        }

        var resources = Resources.Create(Resources.IdentityResources, apiResourcesToKeep, apiScopesToKeep, Resources.OfflineAccess);

        return new ResourceValidationResult
        {
            Resources = resources,
            ParsedScopes = parsedScopesToKeep
        };
    }
}