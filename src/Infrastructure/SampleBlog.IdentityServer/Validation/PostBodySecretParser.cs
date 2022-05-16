using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Parses a POST body for secrets
/// </summary>
public class PostBodySecretParser : ISecretParser
{
    private readonly ILogger logger;
    private readonly IdentityServerOptions options;

    /// <summary>
    /// Returns the authentication method name that this parser implements
    /// </summary>
    /// <value>
    /// The authentication method.
    /// </value>
    public string AuthenticationMethod => OidcConstants.EndpointAuthenticationMethods.PostBody;

    /// <summary>
    /// Creates the parser with options
    /// </summary>
    /// <param name="options">IdentityServer options</param>
    /// <param name="logger">Logger</param>
    public PostBodySecretParser(
        IdentityServerOptions options,
        ILogger<PostBodySecretParser> logger)
    {
        this.logger = logger;
        this.options = options;
    }

    /// <summary>
    /// Tries to find a secret on the context that can be used for authentication
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>
    /// A parsed secret
    /// </returns>
    public async Task<ParsedSecret?> ParseAsync(HttpContext context)
    {
        logger.LogDebug("Start parsing for secret in post body");

        if (!context.Request.IsWebForm())
        {
            logger.LogDebug("Content type is not a form");
            return null;
        }

        var form = await context.Request.ReadFormAsync();

        if (null != form)
        {
            var id = form["client_id"].FirstOrDefault();
            var secret = form["client_secret"].FirstOrDefault();

            // client id must be present
            if (id.IsPresent())
            {
                if (id.Length > options.InputLengthRestrictions.ClientId)
                {
                    logger.LogError("Client ID exceeds maximum length.");
                    return null;
                }

                if (secret.IsPresent())
                {
                    if (secret.Length > options.InputLengthRestrictions.ClientSecret)
                    {
                        logger.LogError("Client secret exceeds maximum length.");
                        return null;
                    }

                    return new ParsedSecret
                    {
                        Id = id,
                        Credential = secret,
                        Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
                    };
                }
                else
                {
                    // client secret is optional
                    logger.LogDebug("client id without secret found");

                    return new ParsedSecret
                    {
                        Id = id,
                        Type = IdentityServerConstants.ParsedSecretTypes.NoSecret
                    };
                }
            }
        }

        logger.LogDebug("No secret in post body found");

        return null;
    }
}