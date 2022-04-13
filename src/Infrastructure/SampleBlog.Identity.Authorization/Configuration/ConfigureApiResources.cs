using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleBlog.Identity.Authorization.Options;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.Identity.Authorization.Configuration;

internal class ConfigureApiResources : IConfigureOptions<ApiAuthorizationOptions>
{
    private const char ScopesSeparator = ' ';

    private readonly IConfiguration configuration;
    private readonly IIdentityServerJwtDescriptor localApiDescriptor;
    private readonly ILogger<ConfigureApiResources> logger;

    public ConfigureApiResources(
        IConfiguration configuration,
        IIdentityServerJwtDescriptor localApiDescriptor,
        ILogger<ConfigureApiResources> logger)
    {
        this.configuration = configuration;
        this.localApiDescriptor = localApiDescriptor;
        this.logger = logger;
    }

    public void Configure(ApiAuthorizationOptions options)
    {
        var resources = GetApiResources();

        foreach (var resource in resources)
        {
            options.ApiResources.Add(resource);
        }
    }

    public static ApiResource GetResource(string name, ResourceDefinition definition)
    {
        switch (definition.Profile)
        {
            case ApplicationProfiles.API:
            {
                return GetAPI(name, definition);
            }

            case ApplicationProfiles.IdentityServerJwt:
            {
                return GetLocalAPI(name, definition);
            }

            default:
            {
                throw new InvalidOperationException($"Type '{definition.Profile}' is not supported.");
            }
        }
    }

    internal IEnumerable<ApiResource> GetApiResources()
    {
        var data = configuration.Get<Dictionary<string, ResourceDefinition>>();

        if (null != data)
        {
            foreach (var kvp in data)
            {
                logger.LogInformation(LoggerEventIds.ConfiguringAPIResource, "Configuring API resource '{ApiResourceName}'.", kvp.Key);
                yield return GetResource(kvp.Key, kvp.Value);
            }
        }

        var localResources = localApiDescriptor?.GetResourceDefinitions();

        if (localResources != null)
        {
            foreach (var kvp in localResources)
            {
                logger.LogInformation(LoggerEventIds.ConfiguringLocalAPIResource, "Configuring local API resource '{ApiResourceName}'.", kvp.Key);
                yield return GetResource(kvp.Key, kvp.Value);
            }
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

    private static ApiResource GetAPI(string name, ResourceDefinition definition) =>
        ApiResourceBuilder.ApiResource(name)
            .FromConfiguration()
            .WithAllowedClients(ApplicationProfilesPropertyValues.AllowAllApplications)
            .ReplaceScopes(ParseScopes(definition.Scopes) ?? new[] { name })
            .Build();

    private static ApiResource GetLocalAPI(string name, ResourceDefinition definition) =>
        ApiResourceBuilder.IdentityServerJwt(name)
            .FromConfiguration()
            .WithAllowedClients(ApplicationProfilesPropertyValues.AllowAllApplications)
            .ReplaceScopes(ParseScopes(definition.Scopes) ?? new[] { name })
            .Build();
}