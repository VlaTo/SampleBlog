using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Validation.Results;

/// <summary>
/// Represents the result of scope parsing.
/// </summary>
public class ParsedScopesResult
{
    /// <summary>
    /// The valid parsed scopes.
    /// </summary>
    public ICollection<ParsedScopeValue> ParsedScopes
    {
        init;
        get;
    }

    /// <summary>
    /// The errors encountered while parsing.
    /// </summary>
    public ICollection<ParsedScopeValidationError> Errors
    {
        init;
        get;
    }

    /// <summary>
    /// Indicates if the result of parsing the scopes was successful.
    /// </summary>
    public bool Succeeded => false == Errors.Any();

    public ParsedScopesResult()
    {
        ParsedScopes = new HashSet<ParsedScopeValue>();
        Errors = new HashSet<ParsedScopeValidationError>();
    }
}