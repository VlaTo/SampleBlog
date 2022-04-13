using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleBlog.Identity.Authorization.Options;
using SampleBlog.IdentityServer;

namespace SampleBlog.Identity.Authorization.Configuration;

internal sealed class ConfigureIdentityResources : IConfigureOptions<ApiAuthorizationOptions>
{
    private const char ScopesSeparator = ' ';

    private readonly IConfiguration configuration;
    private readonly ILogger<ConfigureIdentityResources> logger;

    public ConfigureIdentityResources(IConfiguration configuration, ILogger<ConfigureIdentityResources> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public void Configure(ApiAuthorizationOptions options)
    {
        var data = configuration.Get<IdentityResourceDefinition>();

        if (data is { Scopes: { } })
        {
            var scopes = ParseScopes(data.Scopes);

            if (scopes is { Length: > 0 })
            {
                ClearDefaultIdentityResources(options);
            }

            foreach (var scope in scopes)
            {
                switch (scope)
                {
                    case IdentityServerConstants.StandardScopes.OpenId:
                    {
                        options.IdentityResources.Add(IdentityResourceBuilder.OpenId()
                            .AllowAllClients()
                            .FromConfiguration()
                            .Build());
                        break;
                    }

                    case IdentityServerConstants.StandardScopes.Profile:
                    {
                        options.IdentityResources.Add(IdentityResourceBuilder.Profile()
                            .AllowAllClients()
                            .FromConfiguration()
                            .Build());
                        break;
                    }

                    /*case IdentityServerConstants.StandardScopes.Address:
                    {
                        options.IdentityResources.Add(IdentityResourceBuilder.Address()
                            .AllowAllClients()
                            .FromConfiguration()
                            .Build());
                        break;
                    }

                    case IdentityServerConstants.StandardScopes.Email:
                    {
                        options.IdentityResources.Add(IdentityResourceBuilder.Email()
                            .AllowAllClients()
                            .FromConfiguration()
                            .Build());
                        break;
                    }

                    case IdentityServerConstants.StandardScopes.Phone:
                    {
                        options.IdentityResources.Add(IdentityResourceBuilder.Phone()
                            .AllowAllClients()
                            .FromConfiguration()
                            .Build());
                        break;
                    }*/

                    default:
                    {
                        throw new InvalidOperationException($"Invalid identity resource name '{scope}'");
                    }
                }
            }
        }
    }

    private static void ClearDefaultIdentityResources(ApiAuthorizationOptions options)
    {
        var allDefault = true;

        foreach (var resource in options.IdentityResources)
        {
            var exists = !resource.Properties.TryGetValue(ApplicationProfilesPropertyNames.Source, out var source) ||
                    !String.Equals(ApplicationProfilesPropertyValues.Default, source, StringComparison.OrdinalIgnoreCase);

            if (exists)
            {
                allDefault = false;
                break;
            }
        }

        if (allDefault)
        {
            options.IdentityResources.Clear();
        }
    }

    private static string[]? ParseScopes(string? scopes)
    {
        if (null == scopes)
        {
            return null;
        }

        var parsed = scopes.Split(ScopesSeparator, StringSplitOptions.RemoveEmptyEntries);

        if (0 == parsed.Length)
        {
            return null;
        }

        return parsed;
    }
}