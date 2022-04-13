using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Storage.Stores;

/// <summary>
/// Retrieval of client configuration
/// </summary>
public interface IClientStore
{
    /// <summary>
    /// Finds a client by id
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>The client</returns>
    Task<Client?> FindClientByIdAsync(string clientId);
}