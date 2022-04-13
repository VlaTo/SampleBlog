using Microsoft.Extensions.Options;
using SampleBlog.Identity.Authorization.Options;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.Identity.Authorization.Configuration;

internal sealed class ConfigureApiScopes : IPostConfigureOptions<ApiAuthorizationOptions>
{
    public void PostConfigure(string name, ApiAuthorizationOptions options)
    {
        AddResourceScopesToApiScopes(options);
    }

    private static void AddResourceScopesToApiScopes(ApiAuthorizationOptions options)
    {
        foreach (var resource in options.ApiResources)
        {
            foreach (var scope in resource.Scopes)
            {
                if (options.ApiScopes.ContainsScope(scope))
                {
                    continue;
                }

                options.ApiScopes.Add(new ApiScope(scope));
            }
        }
    }
}