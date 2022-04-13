namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// Models general storage for an external authentication provider/handler scheme
/// </summary>
public class IdentityProvider
{
    /// <summary>
    /// Scheme name for the provider.
    /// </summary>
    public string Scheme
    {
        get;
        set;
    }

    /// <summary>
    /// Display name for the provider.
    /// </summary>
    public string DisplayName
    {
        get;
        set;
    }

    /// <summary>
    /// Flag that indicates if the provider should be used.
    /// </summary>
    public bool Enabled
    {
        get;
        set;
    }

    /// <summary>
    /// Protocol type of the provider.
    /// </summary>
    public string Type
    {
        get;
        set;
    }

    /// <summary>
    /// Protocol specific properties for the provider.
    /// </summary>
    public Dictionary<string, string> Properties
    {
        get;
    }

    /// <summary>
    /// Properties indexer
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    protected string this[string name]
    {
        get
        {
            Properties.TryGetValue(name, out var result);
            return result;
        }
        set
        {
            Properties[name] = value;
        }
    }

    /// <summary>
    /// Ctor
    /// </summary>
    public IdentityProvider(string type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Enabled = true;
    }

    /// <summary>
    /// Ctor
    /// </summary>
    public IdentityProvider(string type, IdentityProvider other)
        : this(type)
    {
        if (null == other)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (other.Type != type)
        {
            throw new ArgumentException($"Type '{type}' does not match type of other '{other.Type}'");
        }

        Scheme = other.Scheme;
        DisplayName = other.DisplayName;
        Enabled = other.Enabled;
        Type = other.Type;
        Properties = new Dictionary<string, string>(other.Properties);
    }
}