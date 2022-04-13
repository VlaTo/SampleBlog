﻿using Microsoft.AspNetCore.Http;

namespace SampleBlog.Identity.Authorization.Core;

/// <summary>
/// Generates oauth/openID parameter values for configured clients.
/// </summary>
public interface IClientRequestParametersProvider
{
    /// <summary>
    /// Gets parameter values for the client with client id<paramref name="clientId"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <param name="clientId">The client id for the client.</param>
    /// <returns>A <see cref="IDictionary{TKey, TValue}"/> containing the client parameters and their values.</returns>
    Task<IDictionary<string, string>> GetClientParametersAsync(HttpContext context, string clientId);
}