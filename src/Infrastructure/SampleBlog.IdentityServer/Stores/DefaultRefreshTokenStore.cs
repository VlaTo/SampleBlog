using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Storage.Stores.Serialization;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// Default refresh token store.
/// </summary>
public class DefaultRefreshTokenStore : DefaultGrantStore<RefreshToken>, IRefreshTokenStore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRefreshTokenStore"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    /// <param name="logger">The logger.</param>
    public DefaultRefreshTokenStore(
        IPersistedGrantStore store,
        IPersistentGrantSerializer serializer,
        IHandleGenerationService handleGenerationService,
        ILogger<DefaultRefreshTokenStore> logger)
        : base(
            IdentityServerConstants.PersistedGrantTypes.RefreshToken,
            store,
            serializer,
            handleGenerationService,
            logger)
    {
    }

    /// <summary>
    /// Stores the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns></returns>
    public async Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultRefreshTokenStore.StoreRefreshTokenAsync");

        return await CreateItemAsync(
            refreshToken,
            refreshToken.ClientId,
            refreshToken.SubjectId!,
            refreshToken.SessionId!,
            refreshToken.Description,
            refreshToken.CreationTime,
            refreshToken.Lifetime
        );
    }

    /// <summary>
    /// Updates the refresh token.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns></returns>
    public Task UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultRefreshTokenStore.UpdateRefreshToken");

        return StoreItemAsync(
            handle,
            refreshToken,
            refreshToken.ClientId,
            refreshToken.SubjectId,
            refreshToken.SessionId,
            refreshToken.Description,
            refreshToken.CreationTime,
            refreshToken.CreationTime + refreshToken.Lifetime,
            refreshToken.ConsumedTime
        );
    }

    /// <summary>
    /// Gets the refresh token.
    /// </summary>
    /// <param name="refreshTokenHandle">The refresh token handle.</param>
    /// <returns></returns>
    public async Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultRefreshTokenStore.GetRefreshToken");

        var refreshToken = await GetItemAsync(refreshTokenHandle);

        if (refreshToken is { Version: < 5 })
        {
#pragma warning disable CS0618 // Type or member is obsolete

            Debugger.Break();

            /*var user = new IdentityServerUser(refreshToken.AccessToken.SubjectId);

            if (null != refreshToken.AccessToken.Claims)
            {
                foreach (var claim in refreshToken.AccessToken.Claims)
                {
                    user.AdditionalClaims.Add(claim);
                }
            }

            refreshToken.Subject = user.CreatePrincipal();
            refreshToken.ClientId = refreshToken.AccessToken.ClientId;
            refreshToken.Description = refreshToken.AccessToken.Description;
            refreshToken.AuthorizedScopes = refreshToken.AccessToken.Scopes;
            refreshToken.SetAccessToken(refreshToken.AccessToken);
            refreshToken.AccessToken = null;
            refreshToken.Version = 5;*/

#pragma warning restore CS0618 // Type or member is obsolete
        }

        return refreshToken;
    }

    /// <summary>
    /// Removes the refresh token.
    /// </summary>
    /// <param name="refreshTokenHandle">The refresh token handle.</param>
    /// <returns></returns>
    public Task RemoveRefreshTokenAsync(string refreshTokenHandle)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultRefreshTokenStore.RemoveRefreshToken");

        return RemoveItemAsync(refreshTokenHandle);
    }

    /// <summary>
    /// Removes the refresh tokens.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns></returns>
    public Task RemoveRefreshTokensAsync(string subjectId, string clientId)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("DefaultRefreshTokenStore.RemoveRefreshTokens");

        return RemoveAllAsync(subjectId, clientId);
    }
}