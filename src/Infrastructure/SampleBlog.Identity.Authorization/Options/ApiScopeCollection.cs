﻿using System.Collections.ObjectModel;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.Identity.Authorization.Options;

/// <summary>
/// A collection of <see cref="ApiScope"/>.
/// </summary>
public sealed class ApiScopeCollection : Collection<ApiScope>
{
    /// <summary>
    /// Gets an API resource given its name.
    /// </summary>
    /// <param name="key">The name of the <see cref="ApiScope"/>.</param>
    /// <returns>The <see cref="ApiScope"/>.</returns>
    public ApiScope this[string key]
    {
        get
        {
            for (var index = 0; index < Items.Count; index++)
            {
                var candidate = Items[index];

                if (string.Equals(candidate.Name, key, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException($"ApiScope '{key}' not found.");
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApiScopeCollection"/>.
    /// </summary>
    public ApiScopeCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApiScopeCollection"/> with the given
    /// API scopes in <paramref name="list"/>.
    /// </summary>
    /// <param name="list">The initial list of <see cref="ApiScope"/>.</param>
    public ApiScopeCollection(IList<ApiScope> list)
        : base(list)
    {
    }

    /// <summary>
    /// Gets whether a given scope is defined or not.
    /// </summary>
    /// <param name="key">The name of the <see cref="ApiScope"/>.</param>
    /// <returns><c>true</c> when the scope is defined; <c>false</c> otherwise.</returns>
    public bool ContainsScope(string key)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            var candidate = Items[i];
            if (string.Equals(candidate.Name, key, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds the scopes in <paramref name="scopes"/> to the collection.
    /// </summary>
    /// <param name="scopes">The list of <see cref="ApiScope"/> to add.</param>
    public void AddRange(params ApiScope[] scopes)
    {
        foreach (var resource in scopes)
        {
            Add(resource);
        }
    }
}