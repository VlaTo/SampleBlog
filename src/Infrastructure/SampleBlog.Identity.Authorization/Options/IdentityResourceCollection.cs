using System.Collections.ObjectModel;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.Identity.Authorization.Options;

public sealed class IdentityResourceCollection : Collection<IdentityResource>
{
    /// <summary>
    /// Gets an identity resource given its name.
    /// </summary>
    /// <param name="key">The name of the <see cref="IdentityResource"/>.</param>
    /// <returns>The <see cref="IdentityResource"/>.</returns>
    public IdentityResource this[string key]
    {
        get
        {
            for (var index = 0; index < Items.Count; index++)
            {
                var candidate = Items[index];

                if (String.Equals(candidate.Name, key, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException($"IdentityResource '{key}' not found.");
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityResourceCollection"/>.
    /// </summary>
    public IdentityResourceCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityResourceCollection"/> with the given
    /// identity resources in <paramref name="list"/>.
    /// </summary>
    /// <param name="list">The initial list of <see cref="IdentityResource"/>.</param>
    public IdentityResourceCollection(IList<IdentityResource> list)
        : base(list)
    {
    }

    /// <summary>
    /// Adds the identity resources in <paramref name="identityResources"/> to the collection.
    /// </summary>
    /// <param name="identityResources">The list of <see cref="IdentityResource"/> to add.</param>
    public void AddRange(params IdentityResource[] identityResources)
    {
        foreach (var resource in identityResources)
        {
            Add(resource);
        }
    }

    /// <summary>
    /// Adds an openid resource.
    /// </summary>
    public void AddOpenId() =>
        Add(IdentityResourceBuilder.OpenId().Build());

    /// <summary>
    /// Adds an openid resource.
    /// </summary>
    /// <param name="configure">The <see cref="Action{IdentityResourceBuilder}"/> to configure the openid scope.</param>
    public void AddOpenId(Action<IdentityResourceBuilder> configure)
    {
        var resource = IdentityResourceBuilder.OpenId();
        
        configure.Invoke(resource);
        
        Add(resource.Build());
    }

    /// <summary>
    /// Adds a profile resource.
    /// </summary>
    public void AddProfile() =>
        Add(IdentityResourceBuilder.Profile().Build());

    /// <summary>
    /// Adds a profile resource.
    /// </summary>
    /// <param name="configure">The <see cref="Action{IdentityResourceBuilder}"/> to configure the profile scope.</param>
    public void AddProfile(Action<IdentityResourceBuilder> configure)
    {
        var resource = IdentityResourceBuilder.Profile();

        configure.Invoke(resource);

        Add(resource.Build());
    }
}