using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Core.Events;
using SampleBlog.IdentityServer.Endpoints.Results;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.ResponseHandling;
using SampleBlog.IdentityServer.ResponseHandling.Models;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Validation;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Endpoints;

/// <summary>
/// The token endpoint
/// </summary>
/// <seealso cref="IEndpointHandler" />
internal sealed class TokenEndpoint : IEndpointHandler
{
    private readonly IClientSecretValidator clientValidator;
    private readonly ITokenRequestValidator requestValidator;
    private readonly ITokenResponseGenerator responseGenerator;
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
        this.responseGenerator = responseGenerator;
        this.events = events;
        this.logger = logger;
    }

    public async Task<IEndpointResult?> ProcessAsync(HttpContext context)
    {
        using var activity = Tracing.ActivitySource.StartActivity(Constants.EndpointNames.Token + "Endpoint");

        logger.LogTrace("Processing token request.");

        // validate HTTP
        if (false == HttpMethods.IsPost(context.Request.Method) || false == context.Request.HasApplicationFormContentType())
        {
            logger.LogWarning("Invalid HTTP request for token endpoint");
            return Error(OidcConstants.TokenErrors.InvalidRequest);
        }

        return await ProcessTokenRequestAsync(context);
    }

    private async Task<IEndpointResult> ProcessTokenRequestAsync(HttpContext context)
    {
        logger.LogDebug("Start token request.");

        // validate client
        var clientResult = await clientValidator.ValidateAsync(context);

        if (null == clientResult.Client)
        {
            return Error(OidcConstants.TokenErrors.InvalidClient);
        }

        // validate request
        var form = (await context.Request.ReadFormAsync()).AsNameValueCollection();

        logger.LogTrace("Calling into token request validator: {type}", requestValidator.GetType().FullName);

        var requestResult = await requestValidator.ValidateRequestAsync(form, clientResult);

        if (requestResult.IsError)
        {
            await events.RaiseAsync(new TokenIssuedFailureEvent(requestResult));
            return Error(requestResult.Error!, requestResult.ErrorDescription, requestResult.CustomResponse);
        }

        // create response
        logger.LogTrace("Calling into token request response generator: {type}", responseGenerator.GetType().FullName);
        var response = await responseGenerator.ProcessAsync(requestResult);

        await events.RaiseAsync(new TokenIssuedSuccessEvent(response, requestResult));

        LogTokens(response, requestResult);

        // return result
        logger.LogDebug("Token request success.");

        return new TokenResult(response);
    }

    private void LogTokens(TokenResponse response, TokenRequestValidationResult requestResult)
    {
        var clientId = $"{requestResult.ValidatedRequest.Client.ClientId} ({requestResult.ValidatedRequest.Client?.ClientName ?? "no name set"})";
        var subjectId = requestResult.ValidatedRequest.Subject?.GetSubjectId() ?? "no subject";

        if (null != response.IdentityToken)
        {
            logger.LogTrace("Identity token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.IdentityToken);
        }

        if (null != response.RefreshToken)
        {
            logger.LogTrace("Refresh token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.RefreshToken);
        }

        if (null != response.AccessToken)
        {
            logger.LogTrace("Access token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.AccessToken);
        }
    }

    private static TokenErrorResult Error(string error, string? errorDescription = null, Dictionary<string, object>? custom = null)
    {
        var response = new TokenErrorResponse
        {
            Error = error,
            ErrorDescription = errorDescription,
            Custom = custom
        };

        return new TokenErrorResult(response);
    }
}