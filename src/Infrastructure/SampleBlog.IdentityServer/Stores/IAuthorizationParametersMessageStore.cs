﻿using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// Interface for authorization request messages that are sent from the authorization endpoint to the login and consent UI.
/// </summary>
public interface IAuthorizationParametersMessageStore
{
    /// <summary>
    /// Writes the authorization parameters.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The identifier for the stored message.</returns>
    Task<string> WriteAsync(Message<IDictionary<string, string[]>> message);

    /// <summary>
    /// Reads the authorization parameters.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task<Message<IDictionary<string, string[]>>?> ReadAsync(string id);

    /// <summary>
    /// Deletes the authorization parameters.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    Task DeleteAsync(string id);
}