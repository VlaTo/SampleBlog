﻿using SampleBlog.IdentityServer.EntityFramework.Storage.Entities;
using ApiResource = SampleBlog.IdentityServer.Storage.Models.ApiResource;
using Secret = SampleBlog.IdentityServer.Storage.Models.Secret;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class ApiResourceExtensions
{
    public static ApiResource UseScopes(this ApiResource apiResource, IList<ApiResourceScope> scopes)
    {
        apiResource.Scopes = new HashSet<string>(scopes.Count);

        for (var index = 0; index < scopes.Count; index++)
        {
            apiResource.Scopes.Add(scopes[index].Scope);
        }

        return apiResource;
    }

    public static ApiResource UseApiSecrets(this ApiResource apiResource, IList<ApiResourceSecret> secrets)
    {
        var apiSecrets = new List<Secret>(secrets.Count);

        for (var index = 0; index < secrets.Count; index++)
        {
            var secret = secrets[index];
            apiSecrets[index] = new Secret(secret.Value, secret.Description, secret.Expiration);
        }

        apiResource.ApiSecrets = apiSecrets;

        return apiResource;
    }

    public static ApiResource UseAllowedAccessTokenSigningAlgorithms(this ApiResource resource, string algorithm)
    {
        var algorithms = algorithm.Split(' ');
        
        resource.AllowedAccessTokenSigningAlgorithms = new HashSet<string>(algorithms);
        
        return resource;
    }
}