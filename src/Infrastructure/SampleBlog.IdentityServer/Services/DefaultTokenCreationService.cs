using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Storage.Models;
using System.Text.Json;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Default token creation service
/// </summary>
public class DefaultTokenCreationService : ITokenCreationService
{
    /// <summary>
    /// The key service
    /// </summary>
    protected IKeyMaterialService Keys
    {
        get;
    }

    /// <summary>
    ///  The clock
    /// </summary>
    protected ISystemClock Clock
    {
        get;
    }

    /// <summary>
    /// The options
    /// </summary>
    protected IdentityServerOptions Options
    {
        get;
    }

    /// <summary>
    /// The logger
    /// </summary>
    protected ILogger Logger
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTokenCreationService"/> class.
    /// </summary>
    /// <param name="clock">The options.</param>
    /// <param name="keys">The keys.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public DefaultTokenCreationService(
        ISystemClock clock,
        IKeyMaterialService keys,
        IdentityServerOptions options,
        ILogger<DefaultTokenCreationService> logger)
    {
        Clock = clock;
        Keys = keys;
        Options = options;
        Logger = logger;
    }

    /// <summary>
    /// Creates the token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>
    /// A protected and serialized security token
    /// </returns>
    public virtual async Task<string> CreateTokenAsync(Token token)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultTokenCreationService.CreateToken");

        var payload = await CreatePayloadAsync(token);
        var headerElements = await CreateHeaderElementsAsync(token);

        return await CreateJwtAsync(token, payload, headerElements);
    }

    /// <summary>
    /// Creates the JWT payload
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    protected virtual Task<string> CreatePayloadAsync(Token token)
    {
        var payload = token.CreateJwtPayloadDictionary(Options, Clock, Logger);
        return Task.FromResult(JsonSerializer.Serialize(payload));
    }

    /// <summary>
    /// Creates additional JWT header elements
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    protected virtual Task<Dictionary<string, object>> CreateHeaderElementsAsync(Token token)
    {
        var additionalHeaderElements = new Dictionary<string, object>();

        if (IdentityServerConstants.TokenTypes.AccessToken == token.Type)
        {
            if (Options.AccessTokenJwtType.IsPresent())
            {
                additionalHeaderElements.Add("typ", Options.AccessTokenJwtType);
            }
        }

        return Task.FromResult(additionalHeaderElements);
    }

    /// <summary>
    /// Creates JWT token
    /// </summary>
    /// <param name="token"></param>
    /// <param name="payload"></param>
    /// <param name="headerElements"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected virtual async Task<string> CreateJwtAsync(Token token, string payload, Dictionary<string, object> headerElements)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultTokenCreationService.CreateJwt");

        var credential = await Keys.GetSigningCredentialsAsync(token.AllowedSigningAlgorithms);

        if (null == credential)
        {
            throw new InvalidOperationException("No signing credential is configured. Can't create JWT token");
        }

        var handler = new JsonWebTokenHandler
        {
            SetDefaultTimesOnTokenCreation = false
        };

        return handler.CreateToken(payload, credential, headerElements);
    }
}