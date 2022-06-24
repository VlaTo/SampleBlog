using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Extensions;

public static class ClientExtensions
{
    /// <summary>
    /// Returns true if the client is an implicit-only client.
    /// </summary>
    public static bool IsImplicitOnly(this Client? client)
    {
        return client is { AllowedGrantTypes.Count: 1 } && GrantType.Implicit == client.AllowedGrantTypes.First();
    }
}