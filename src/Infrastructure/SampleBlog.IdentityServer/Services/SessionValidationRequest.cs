using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Models request to validation a session from a client.
/// </summary>
public class SessionValidationRequest
{
    /// <summary>
    /// The subject ID
    /// </summary>
    public string SubjectId
    {
        get;
        set;
    }

    /// <summary>
    /// The session ID
    /// </summary>
    public string SessionId
    {
        get;
        set;
    }

    /// <summary>
    /// The client making the request.
    /// </summary>
    public Client Client
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates the type of request.
    /// </summary>
    public SessionValidationType Type
    {
        get;
        set;
    }
}