using System.Diagnostics;

namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// Models access to an API scope
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class ApiScope : Resource
{
    /// <summary>
    /// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
    /// </summary>
    public bool Required
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
    /// </summary>
    public bool Emphasize
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiScope"/> class.
    /// </summary>
    /*public ApiScope()
    {
    }*/

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiScope"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public ApiScope(string name)
        : this(name, name, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiScope"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    public ApiScope(string name, string displayName)
        : this(name, displayName, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiScope"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="userClaims">List of associated user claims that should be included when this resource is requested.</param>
    public ApiScope(string name, IEnumerable<string> userClaims)
        : this(name, name, userClaims)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiScope"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="userClaims">List of associated user claims that should be included when this resource is requested.</param>
    /// <exception cref="System.ArgumentNullException">name</exception>
    public ApiScope(string name, string displayName, IEnumerable<string>? userClaims)
        : base(name, displayName)
    {
        Required = false;
        Emphasize = false;

        if (null != userClaims)
        {
            foreach (var type in userClaims)
            {
                UserClaims.Add(type);
            }
        }
    }

    #region DebugDisplay

    private string DebuggerDisplay => Name ?? $"{{{typeof(ApiScope)}}}";

    #endregion
}