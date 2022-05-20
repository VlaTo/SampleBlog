﻿using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Default implementation of IResourceValidator.
/// </summary>
public class DefaultResourceValidator : IResourceValidator
{
    private readonly ILogger logger;
    private readonly IScopeParser scopeParser;
    private readonly IResourceStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultResourceValidator"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="scopeParser"></param>
    /// <param name="logger">The logger.</param>
    public DefaultResourceValidator(
        IResourceStore store,
        IScopeParser scopeParser,
        ILogger<DefaultResourceValidator> logger)
    {
        this.store = store;
        this.scopeParser = scopeParser;
        this.logger = logger;
    }

    public virtual async Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request)
    {
        using var activity = Tracing.ValidationActivitySource.StartActivity("DefaultResourceValidator.ValidateRequestedResources");

        activity?.SetTag(Tracing.Properties.Scope, request.Scopes.ToSpaceSeparatedString());
        activity?.SetTag(Tracing.Properties.Resource, request.ResourceIndicators.ToSpaceSeparatedString());

        /*if (null == request)
        {
            throw new ArgumentNullException(nameof(request));
        }*/

        var result = new ResourceValidationResult();
        var parsedScopesResult = scopeParser.ParseScopeValues(request.Scopes);

        if (false == parsedScopesResult.Succeeded)
        {
            foreach (var invalidScope in parsedScopesResult.Errors)
            {
                logger.LogError("Invalid parsed scope {scope}, message: {error}", invalidScope.RawValue, invalidScope.Error);
                result.InvalidScopes.Add(invalidScope.RawValue);
            }

            return result;
        }

        var scopeNames = parsedScopesResult.ParsedScopes
            .Select(x => x.ParsedName)
            .Distinct()
            .ToArray();

        // todo: this API might want to pass resource indicators to better filter
        var scopeResourcesFromStore = await store.FindEnabledResourcesByScopeAsync(scopeNames);
        
        Resources? scopeResources;

        if (request.ResourceIndicators.Any())
        {
            // remove isolated API resources not included in the requested resource indicators
            var apiResources = scopeResourcesFromStore.ApiResources
                .Where(x =>
                    // only allow non-isolated resources if the request could produce multiple access
                    // tokens. this will happen if the request is for a RT, so check for offline_access
                    (request.IncludeNonIsolatedApiResources && false == x.RequireResourceIndicator) || request.ResourceIndicators.Contains(x.Name)
                )
                .ToHashSet();

            scopeResources = Resources.Create(
                scopeResourcesFromStore.IdentityResources,
                apiResources,
                scopeResourcesFromStore.ApiScopes,
                scopeResourcesFromStore.OfflineAccess
            );

            if (false == request.IncludeNonIsolatedApiResources)
            {
                // filter API scopes that don't match the resources requested
                var allResourceScopes = scopeResourcesFromStore.ApiResources
                    .SelectMany(x => x.Scopes)
                    .ToArray();
                //scopeResourcesFromStore.ApiScopes =
                var apiScopes = scopeResourcesFromStore.ApiScopes
                    .Where(x => allResourceScopes.Contains(x.Name))
                    .ToHashSet();

                scopeResources = Resources.Create(
                    scopeResourcesFromStore.IdentityResources,
                    scopeResourcesFromStore.ApiResources,
                    apiScopes,
                    scopeResourcesFromStore.OfflineAccess
                );
            }

            // find requested resource indicators not matched by scope
            var matchedApiResourceNames = scopeResourcesFromStore.ApiResources.Select(x => x.Name).ToArray();
            var invalidRequestedResourceIndicators = request.ResourceIndicators.Except(matchedApiResourceNames);

            if (invalidRequestedResourceIndicators.Any())
            {
                foreach (var invalid in invalidRequestedResourceIndicators)
                {
                    logger.LogError("Invalid resource identifier {resource}. It is either not found, not enabled, or does not support any of the requested scopes.", invalid);
                    result.InvalidResourceIndicators.Add(invalid);
                }

                return result;
            }
        }
        else
        {
            // no resource indicators, so filter all API resources marked as isolated
            //scopeResourcesFromStore.ApiResources = scopeResourcesFromStore.ApiResources
            var apiResources = scopeResourcesFromStore.ApiResources
                .Where(x => false == x.RequireResourceIndicator)
                .ToHashSet();

            scopeResources = Resources.Create(
                scopeResourcesFromStore.IdentityResources,
                apiResources,
                scopeResourcesFromStore.ApiScopes,
                scopeResourcesFromStore.OfflineAccess
            );
        }

        foreach (var scope in parsedScopesResult.ParsedScopes)
        {
            await ValidateScopeAsync(request.Client, scopeResources, scope, result);
        }
        
        if (0 < result.InvalidScopes.Count || 0 < result.InvalidResourceIndicators.Count)
        {
            result.Resources.IdentityResources.Clear();
            result.Resources.ApiResources.Clear();
            result.Resources.ApiScopes.Clear();
            result.ParsedScopes.Clear();
        }
        
        return result;
    }

    /// <summary>
    /// Validates that the requested scopes is contained in the store, and the client is allowed to request it.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="resourcesFromStore"></param>
    /// <param name="requestedScope"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected virtual async Task ValidateScopeAsync(
        Client client,
        Resources resourcesFromStore,
        ParsedScopeValue requestedScope,
        ResourceValidationResult result)
    {
        if (IdentityServerConstants.StandardScopes.OfflineAccess == requestedScope.ParsedName)
        {
            if (await IsClientAllowedOfflineAccessAsync(client))
            {
                //result.Resources.OfflineAccess = true;
                result.ParsedScopes.Add(
                    new ParsedScopeValue(IdentityServerConstants.StandardScopes.OfflineAccess)
                );
            }
            else
            {
                result.InvalidScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
            }
        }
        else
        {
            var identity = resourcesFromStore.FindIdentityResourcesByScope(requestedScope.ParsedName);

            if (null != identity)
            {
                if (await IsClientAllowedIdentityResourceAsync(client, identity))
                {
                    result.ParsedScopes.Add(requestedScope);
                    result.Resources.IdentityResources.Add(identity);
                }
                else
                {
                    result.InvalidScopes.Add(requestedScope.RawValue);
                }
            }
            else
            {
                var apiScope = resourcesFromStore.FindApiScope(requestedScope.ParsedName);

                if (null != apiScope)
                {
                    if (await IsClientAllowedApiScopeAsync(client, apiScope))
                    {
                        result.ParsedScopes.Add(requestedScope);
                        result.Resources.ApiScopes.Add(apiScope);

                        var apis = resourcesFromStore.FindApiResourcesByScope(apiScope.Name);

                        foreach (var api in apis)
                        {
                            result.Resources.ApiResources.Add(api);
                        }
                    }
                    else
                    {
                        result.InvalidScopes.Add(requestedScope.RawValue);
                    }
                }
                else
                {
                    logger.LogError("Scope {scope} not found in store or not supported by requested resource indicators.", requestedScope.ParsedName);
                    result.InvalidScopes.Add(requestedScope.RawValue);
                }
            }
        }
    }

    /// <summary>
    /// Determines if client is allowed access to the identity scope.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="identity"></param>
    /// <returns></returns>
    protected virtual Task<bool> IsClientAllowedIdentityResourceAsync(Client client, IdentityResource identity)
    {
        var allowed = client.AllowedScopes.Contains(identity.Name);

        if (!allowed)
        {
            logger.LogError("Client {client} is not allowed access to scope {scope}.", client.ClientId, identity.Name);
        }

        return Task.FromResult(allowed);
    }

    /// <summary>
    /// Determines if client is allowed access to the API scope.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="apiScope"></param>
    /// <returns></returns>
    protected virtual Task<bool> IsClientAllowedApiScopeAsync(Client client, ApiScope apiScope)
    {
        var allowed = client.AllowedScopes.Contains(apiScope.Name);

        if (!allowed)
        {
            logger.LogError("Client {client} is not allowed access to scope {scope}.", client.ClientId, apiScope.Name);
        }

        return Task.FromResult(allowed);
    }

    /// <summary>
    /// Validates if the client is allowed offline_access.
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    protected virtual Task<bool> IsClientAllowedOfflineAccessAsync(Client client)
    {
        var allowed = client.AllowOfflineAccess;

        if (!allowed)
        {
            logger.LogError("Client {client} is not allowed access to scope offline_access (via AllowOfflineAccess setting).", client.ClientId);
        }

        return Task.FromResult(allowed);
    }
}