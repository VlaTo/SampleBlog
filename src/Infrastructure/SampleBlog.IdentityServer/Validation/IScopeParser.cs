using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Allows parsing raw scopes values into structured scope values.
/// </summary>
public interface IScopeParser
{
    // todo: test return no error, and no parsed scopes. how do callers behave?
    /// <summary>
    /// Parses the requested scopes.
    /// </summary>
    ParsedScopesResult ParseScopeValues(IEnumerable<string> scopeValues);
}