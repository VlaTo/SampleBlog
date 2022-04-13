using System.Diagnostics;
using SampleBlog.IdentityServer.Core;

namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// Models the common data of API and identity resources.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Resource
{
    /// <summary>
    /// Indicates if this resource is enabled. Defaults to true.
    /// </summary>
    public bool Enabled
    {
        get;
        set;
    }

    /// <summary>
    /// The unique name of the resource.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Display name of the resource.
    /// </summary>
    public string DisplayName
    {
        get;
        set;
    }

    /// <summary>
    /// Description of the resource.
    /// </summary>
    public string? Description
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether this scope is shown in the discovery document. Defaults to true.
    /// </summary>
    public bool ShowInDiscoveryDocument
    {
        get;
        set;
    }

    /// <summary>
    /// List of associated user claims that should be included when this resource is requested.
    /// </summary>
    public ICollection<string> UserClaims
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the custom properties for the resource.
    /// </summary>
    /// <value>
    /// The properties.
    /// </value>
    public IDictionary<string, string> Properties
    {
        get;
        set;
    }

    public Resource(string name, string displayName)
    {
        Throw.IfNull(name, nameof(name));
        Throw.IfNull(displayName, nameof(displayName));

        Name = name!;
        DisplayName = displayName!;
        Enabled = true;
        ShowInDiscoveryDocument = true;
        UserClaims = new HashSet<string>();
        Properties = new Dictionary<string, string>();
    }

    #region DebugDisplay

    private string DebuggerDisplay => Name ?? $"{{{typeof(Resource)}}}";

    #endregion
}