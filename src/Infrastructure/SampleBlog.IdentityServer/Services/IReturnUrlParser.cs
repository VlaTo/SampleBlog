﻿using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Interface for the return URL parser
/// </summary>
public interface IReturnUrlParser
{
    /// <summary>
    /// Parses a return URL.
    /// </summary>
    /// <param name="returnUrl">The return URL.</param>
    /// <returns></returns>
    Task<AuthorizationRequest> ParseAsync(string returnUrl);

    /// <summary>
    /// Determines whether the return URL is valid.
    /// </summary>
    /// <param name="returnUrl">The return URL.</param>
    /// <returns>
    ///   <c>true</c> if the return URL is valid; otherwise, <c>false</c>.
    /// </returns>
    bool IsValidReturnUrl(string? returnUrl);
}