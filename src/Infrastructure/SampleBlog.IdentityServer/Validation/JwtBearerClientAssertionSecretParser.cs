using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using System.IdentityModel.Tokens.Jwt;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Parses a POST body for a JWT bearer client assertion
/// </summary>
public class JwtBearerClientAssertionSecretParser : ISecretParser
{
    private readonly IdentityServerOptions options;
    private readonly ILogger logger;

    /// <summary>
    /// Returns the authentication method name that this parser implements
    /// </summary>
    /// <value>
    /// The authentication method.
    /// </value>
    public string AuthenticationMethod => OidcConstants.EndpointAuthenticationMethods.PrivateKeyJwt;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtBearerClientAssertionSecretParser"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public JwtBearerClientAssertionSecretParser(
        IdentityServerOptions options,
        ILogger<JwtBearerClientAssertionSecretParser> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    /// <summary>
    /// Tries to find a JWT client assertion token in the request body that can be used for authentication
    /// Used for "private_key_jwt" client authentication method as defined in http://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>
    /// A parsed secret
    /// </returns>
    public async Task<ParsedSecret?> ParseAsync(HttpContext context)
    {
        logger.LogDebug("Start parsing for JWT client assertion in post body");

        if (false == context.Request.HasApplicationFormContentType())
        {
            logger.LogDebug("Content type is not a form");

            return null;
        }

        var body = await context.Request.ReadFormAsync();

        if (null != body)
        {
            var clientAssertionType = body[OidcConstants.TokenRequest.ClientAssertionType].FirstOrDefault();
            var clientAssertion = body[OidcConstants.TokenRequest.ClientAssertion].FirstOrDefault();

            if (clientAssertion.IsPresent()
                && clientAssertionType == OidcConstants.ClientAssertionTypes.JwtBearer)
            {
                if (clientAssertion.Length > options.InputLengthRestrictions.Jwt)
                {
                    logger.LogError("Client assertion token exceeds maximum length.");

                    return null;
                }

                var clientId = GetClientIdFromToken(clientAssertion);

                if (String.IsNullOrEmpty(clientId))
                {
                    return null;
                }

                if (clientId.Length > options.InputLengthRestrictions.ClientId)
                {
                    logger.LogError("Client ID exceeds maximum length.");

                    return null;
                }

                var parsedSecret = new ParsedSecret
                {
                    Id = clientId,
                    Credential = clientAssertion,
                    Type = IdentityServerConstants.ParsedSecretTypes.JwtBearer
                };

                return parsedSecret;
            }
        }

        logger.LogDebug("No JWT client assertion found in post body");

        return null;
    }
    
    private string? GetClientIdFromToken(string token)
    {
        try
        {
            var jwt = new JwtSecurityToken(token);
            return jwt.Subject;
        }
        catch (Exception e)
        {
            logger.LogWarning("Could not parse client assertion", e);

            return null;
        }
    }
}