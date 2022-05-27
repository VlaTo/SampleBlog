﻿using System.Diagnostics;

namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// Models a user identity resource.
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class IdentityResource : Resource
{
    /// <summary>
    /// Specifies whether the user can de-select the scope on the consent screen (if the consent screen wants to implement such a feature).
    /// Defaults to false.
    /// </summary>
    public bool Required
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether the consent screen will emphasize this scope (if the consent screen wants to implement such a feature). 
    /// Use this setting for sensitive or important scopes. Defaults to false.
    /// </summary>
    public bool Emphasize
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityResource"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="userClaims">List of associated user claims that should be included when this resource is requested.</param>
    public IdentityResource(string name)
        : base(name, name)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityResource"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="userClaims">List of associated user claims that should be included when this resource is requested.</param>
    /// <exception cref="System.ArgumentNullException">name</exception>
    /// <exception cref="System.ArgumentException">Must provide at least one claim type - claimTypes</exception>
    public IdentityResource(string name, string displayName)
        : base(name, displayName)
    {
    }

    #region DebugDisplay

    private string DebuggerDisplay => Name ?? $"{{{typeof(IdentityResource)}}}";

    #endregion
}