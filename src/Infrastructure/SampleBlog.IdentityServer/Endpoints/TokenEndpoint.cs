using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Validation;

namespace SampleBlog.IdentityServer.Endpoints;

/// <summary>
/// The token endpoint
/// </summary>
/// <seealso cref="IEndpointHandler" />
internal sealed class TokenEndpoint : IEndpointHandler
{
    private readonly IClientSecretValidator clientValidator;
    private readonly ITokenRequestValidator requestValidator;
    private readonly ITokenResponseGenerator _responseGenerator;
    private readonly IEventService events;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenEndpoint" /> class.
    /// </summary>
    /// <param name="clientValidator">The client validator.</param>
    /// <param name="requestValidator">The request validator.</param>
    /// <param name="responseGenerator">The response generator.</param>
    /// <param name="events">The events.</param>
    /// <param name="logger">The logger.</param>
    public TokenEndpoint(
        IClientSecretValidator clientValidator,
        ITokenRequestValidator requestValidator,
        ITokenResponseGenerator responseGenerator,
        IEventService events,
        ILogger<TokenEndpoint> logger)
    {
        this.clientValidator = clientValidator;
        this.requestValidator = requestValidator;
        _responseGenerator = responseGenerator;
        this.events = events;
        this.logger = logger;
    }

    public Task<IEndpointResult?> ProcessAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
}