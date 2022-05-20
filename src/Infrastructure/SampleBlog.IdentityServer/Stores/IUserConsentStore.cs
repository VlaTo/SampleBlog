﻿using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// Interface for user consent storage
/// </summary>
public interface IUserConsentStore
{
    /// <summary>
    /// Stores the user consent.
    /// </summary>
    /// <param name="consent">The consent.</param>
    /// <returns></returns>
    Task StoreUserConsentAsync(Consent consent);

    /// <summary>
    /// Gets the user consent.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns></returns>
    Task<Consent?> GetUserConsentAsync(string subjectId, string clientId);

    /// <summary>
    /// Removes the user consent.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns></returns>
    Task RemoveUserConsentAsync(string subjectId, string clientId);
}