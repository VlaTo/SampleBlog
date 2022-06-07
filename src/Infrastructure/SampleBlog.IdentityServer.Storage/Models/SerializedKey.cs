namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// Serialized key.
/// </summary>
public sealed class SerializedKey
{
    /// <summary>
    /// Version number of serialized key.
    /// </summary>
    public int Version
    {
        get;
        set;
    }

    /// <summary>
    /// Key identifier.
    /// </summary>
    public string Id
    {
        get;
        set;
    }

    /// <summary>
    /// Date key was created.
    /// </summary>
    public DateTime Created
    {
        get;
        set;
    }

    /// <summary>
    /// The algorithm.
    /// </summary>
    public string Algorithm
    {
        get;
        set;
    }

    /// <summary>
    /// Contains X509 certificate.
    /// </summary>
    public bool IsX509Certificate
    {
        get;
        set;
    }

    /// <summary>
    /// Serialized data for key.
    /// </summary>
    public string Data
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates if data is protected.
    /// </summary>
    public bool DataProtected
    {
        get;
        set;
    }
}