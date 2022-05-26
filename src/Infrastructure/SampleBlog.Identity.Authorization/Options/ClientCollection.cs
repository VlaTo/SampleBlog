using System.Collections.ObjectModel;
using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

namespace SampleBlog.Identity.Authorization.Options;

/// <summary>
/// A collection of <see cref="Client"/>.
/// </summary>
public class ClientCollection : Collection<IdentityServer.Storage.Models.Client>
{
    private readonly IList<IdentityServer.Storage.Models.Client> list;

    /// <summary>
    /// Gets a client given its client id.
    /// </summary>
    /// <param name="key">The name of the <see cref="Client"/>.</param>
    /// <returns>The <see cref="Client"/>.</returns>
    public IdentityServer.Storage.Models.Client this[string key]
    {
        get
        {
            for (var index = 0; index < Items.Count; index++)
            {
                var candidate = Items[index];

                if (string.Equals(candidate.ClientId, key, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException($"Client '{key}' not found.");
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ClientCollection"/>.
    /// </summary>
    public ClientCollection()
    {
        list = new List<IdentityServer.Storage.Models.Client>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ClientCollection"/> with the given
    /// clients in <paramref name="list"/>.
    /// </summary>
    /// <param name="list">The initial list of <see cref="Client"/>.</param>
    public ClientCollection(IList<IdentityServer.Storage.Models.Client> list)
        : base(list)
    {
        this.list = list;
    }

    /// <summary>
    /// Adds the clients in <paramref name="clients"/> to the collection.
    /// </summary>
    /// <param name="clients">The list of <see cref="Client"/> to add.</param>
    public void AddRange(params IdentityServer.Storage.Models.Client[] clients)
    {
        foreach (var client in clients)
        {
            Add(client);
        }
    }

    /// <summary>
    /// Adds a single page application that coexists with an authorization server.
    /// </summary>
    /// <param name="clientId">The client id for the single page application.</param>
    /// <param name="configure">The <see cref="Action{ClientBuilder}"/> to configure the default single page application.</param>
    public IdentityServer.Storage.Models.Client AddIdentityServerSPA(string clientId, Action<ClientBuilder> configure)
    {
        var app = ClientBuilder.IdentityServerSPA(clientId);
        
        configure.Invoke(app);
        
        var client = app.Build();
        
        Add(client);
        
        return client;
    }

    /// <summary>
    /// Adds an externally registered single page application.
    /// </summary>
    /// <param name="clientId">The client id for the single page application.</param>
    /// <param name="configure">The <see cref="Action{ClientBuilder}"/> to configure the default single page application.</param>
    public IdentityServer.Storage.Models.Client AddSPA(string clientId, Action<ClientBuilder> configure)
    {
        var app = ClientBuilder.SPA(clientId);

        configure.Invoke(app);

        var client = app.Build();

        Add(client);

        return client;
    }

    /// <summary>
    /// Adds an externally registered native application..
    /// </summary>
    /// <param name="clientId">The client id for the single page application.</param>
    /// <param name="configure">The <see cref="Action{ClientBuilder}"/> to configure the native application.</param>
    public IdentityServer.Storage.Models.Client AddNativeApp(string clientId, Action<ClientBuilder> configure)
    {
        var app = ClientBuilder.NativeApp(clientId);

        configure.Invoke(app);

        var client = app.Build();

        Add(client);

        return client;
    }
}