﻿using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Storage.Stores;

/// <summary>
/// Interface for the backchannel authentication request store
/// </summary>
public interface IBackChannelAuthenticationRequestStore
{
    /// <summary>
    /// Creates the request.
    /// </summary>
    Task<string> CreateRequestAsync(BackChannelAuthenticationRequest request);

    /// <summary>
    /// Gets the requests.
    /// </summary>
    Task<IEnumerable<BackChannelAuthenticationRequest>?> GetLoginsForUserAsync(string subjectId, string? clientId = null);

    /// <summary>
    /// Gets the request.
    /// </summary>
    Task<BackChannelAuthenticationRequest?> GetByAuthenticationRequestIdAsync(string requestId);

    /// <summary>
    /// Gets the request.
    /// </summary>
    Task<BackChannelAuthenticationRequest?> GetByInternalIdAsync(string id);

    /// <summary>
    /// Removes the request.
    /// </summary>
    Task RemoveByInternalIdAsync(string id);

    /// <summary>
    /// Updates the request.
    /// </summary>
    Task UpdateByInternalIdAsync(string id, BackChannelAuthenticationRequest request);
}