namespace SampleBlog.IdentityServer.Storage;

/// <summary>
/// Represents a filter used when accessing the persisted grants store. 
/// Setting multiple properties is interpreted as a logical 'AND' to further filter the query.
/// At least one value must be supplied.
/// </summary>
public sealed class PersistedGrantFilter
{
    /// <summary>
    /// Subject id of the user.
    /// </summary>
    public string? SubjectId
    {
        get;
    }

    /// <summary>
    /// Session id used for the grant.
    /// </summary>
    public string? SessionId
    {
        get;
    }

    /// <summary>
    /// Client id the grant was issued to.
    /// </summary>
    public string? ClientId
    {
        get;
    }

    /// <summary>
    /// Client ids the grant was issued to.
    /// </summary>
    public IEnumerable<string>? ClientIds
    {
        get;
    }

    /// <summary>
    /// The type of grant.
    /// </summary>
    public string? Type
    {
        get;
    }

    /// <summary>
    /// The types of grants.
    /// </summary>
    public IEnumerable<string>? Types
    {
        get;
    }

    public PersistedGrantFilter(
        string? subjectId = null,
        string? sessionId = null,
        string? clientId = null,
        IEnumerable<string>? clientIds = null,
        string? type = null,
        IEnumerable<string>? types = null)
    {
        SubjectId = subjectId;
        SessionId = sessionId;
        ClientId = clientId;
        ClientIds = clientIds;
        Type = type;
        Types = types;
    }
}