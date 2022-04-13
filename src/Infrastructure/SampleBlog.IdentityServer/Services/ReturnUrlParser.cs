﻿using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Parses a return URL using all registered URL parsers
/// </summary>
public class ReturnUrlParser
{
    private readonly IEnumerable<IReturnUrlParser> parsers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReturnUrlParser"/> class.
    /// </summary>
    /// <param name="parsers">The parsers.</param>
    public ReturnUrlParser(IEnumerable<IReturnUrlParser> parsers)
    {
        this.parsers = parsers;
    }

    /// <summary>
    /// Parses the return URL.
    /// </summary>
    /// <param name="returnUrl">The return URL.</param>
    /// <returns></returns>
    public virtual async Task<AuthorizationRequest> ParseAsync(string returnUrl)
    {
        using var activity = Tracing.ValidationActivitySource.StartActivity("ReturnUrlParser.Parse");

        foreach (var parser in parsers)
        {
            var result = await parser.ParseAsync(returnUrl);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether a return URL is valid.
    /// </summary>
    /// <param name="returnUrl">The return URL.</param>
    /// <returns>
    ///   <c>true</c> if the return URL is valid; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool IsValidReturnUrl(string returnUrl)
    {
        using var activity = Tracing.ValidationActivitySource.StartActivity("ReturnUrlParser.IsValidReturnUrl");

        foreach (var parser in parsers)
        {
            if (parser.IsValidReturnUrl(returnUrl))
            {
                return true;
            }
        }

        return false;
    }
}