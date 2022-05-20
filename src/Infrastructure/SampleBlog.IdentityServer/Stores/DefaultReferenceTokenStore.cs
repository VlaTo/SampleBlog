﻿using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Storage.Stores.Serialization;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// Default reference token store.
/// </summary>
public class DefaultReferenceTokenStore : DefaultGrantStore<Token>, IReferenceTokenStore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultReferenceTokenStore"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    /// <param name="logger">The logger.</param>
    public DefaultReferenceTokenStore(
        IPersistedGrantStore store,
        IPersistentGrantSerializer serializer,
        IHandleGenerationService handleGenerationService,
        ILogger<DefaultReferenceTokenStore> logger)
        : base(IdentityServerConstants.PersistedGrantTypes.ReferenceToken, store, serializer, handleGenerationService, logger)
    {
    }

    /// <summary>
    /// Stores the reference token asynchronous.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns></returns>
    public Task<string> StoreReferenceTokenAsync(Token token)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultReferenceTokenStore.StoreReferenceToken");

        return CreateItemAsync(token, token.ClientId, token.SubjectId, token.SessionId, token.Description, token.CreationTime, token.Lifetime);
    }

    /// <summary>
    /// Gets the reference token asynchronous.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns></returns>
    public Task<Token> GetReferenceTokenAsync(string handle)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultReferenceTokenStore.GetReferenceToken");

        return GetItemAsync(handle);
    }

    /// <summary>
    /// Removes the reference token asynchronous.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns></returns>
    public Task RemoveReferenceTokenAsync(string handle)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultReferenceTokenStore.RemoveReferenceToken");

        return RemoveItemAsync(handle);
    }

    /// <summary>
    /// Removes the reference tokens asynchronous.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns></returns>
    public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultReferenceTokenStore.RemoveReferenceTokens");

        return RemoveAllAsync(subjectId, clientId);
    }
}