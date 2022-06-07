using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// In-memory client store
/// </summary>
public sealed class InMemoryClientStore : IClientStore
{
    private readonly IEnumerable<Client> clients;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryClientStore"/> class.
    /// </summary>
    /// <param name="clients">The clients.</param>
    public InMemoryClientStore(IEnumerable<Client> clients)
    {
        /*if (clients.HasDuplicates(m => m.ClientId))
        {
            throw new ArgumentException("Clients must not contain duplicate ids");
        }*/

        this.clients = clients;
    }

    public Task<Client?> FindClientByIdAsync(string clientId)
    {
        using var activity = Tracing.ActivitySource.StartActivity("InMemoryClientStore.FindClientById");
        
        activity?.SetTag(Tracing.Properties.ClientId, clientId);

        var result = clients.SingleOrDefault(client => client.ClientId == clientId);

        return Task.FromResult(result);
    }
}