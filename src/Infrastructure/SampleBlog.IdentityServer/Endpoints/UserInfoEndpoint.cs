using System.Net;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Endpoints.Results;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.ResponseHandling;
using SampleBlog.IdentityServer.Validation;

namespace SampleBlog.IdentityServer.Endpoints;

/// <summary>
/// The userinfo endpoint
/// </summary>
/// <seealso cref="IEndpointHandler" />
internal sealed class UserInfoEndpoint : IEndpointHandler
{
    private readonly BearerTokenUsageValidator tokenUsageValidator;
    private readonly IUserInfoRequestValidator requestValidator;
    private readonly IUserInfoResponseGenerator responseGenerator;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoEndpoint" /> class.
    /// </summary>
    /// <param name="tokenUsageValidator">The token usage validator.</param>
    /// <param name="requestValidator">The request validator.</param>
    /// <param name="responseGenerator">The response generator.</param>
    /// <param name="logger">The logger.</param>
    public UserInfoEndpoint(
        BearerTokenUsageValidator tokenUsageValidator,
        IUserInfoRequestValidator requestValidator,
        IUserInfoResponseGenerator responseGenerator,
        ILogger<UserInfoEndpoint> logger)
    {
        this.tokenUsageValidator = tokenUsageValidator;
        this.requestValidator = requestValidator;
        this.responseGenerator = responseGenerator;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns></returns>
    public async Task<IEndpointResult?> ProcessAsync(HttpContext context)
    {
        using var activity = Tracing.BasicActivitySource.StartActivity(Constants.EndpointNames.UserInfo + "Endpoint");

        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsPost(context.Request.Method))
        {
            logger.LogWarning("Invalid HTTP method for userinfo endpoint.");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        return await ProcessUserInfoRequestAsync(context);
    }

    private async Task<IEndpointResult> ProcessUserInfoRequestAsync(HttpContext context)
    {
        logger.LogDebug("Start userinfo request");

        // userinfo requires an access token on the request
        var tokenUsageResult = await tokenUsageValidator.ValidateAsync(context);

        if (false == tokenUsageResult.TokenFound)
        {
            var error = "No access token found.";

            logger.LogError(error);
            return Error(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        // validate the request
        logger.LogTrace("Calling into userinfo request validator: {type}", requestValidator.GetType().FullName);

        var validationResult = await requestValidator.ValidateRequestAsync(tokenUsageResult.Token);

        if (validationResult.IsError)
        {
            //_logger.LogError("Error validating  validationResult.Error);
            return Error(validationResult.Error!);
        }

        // generate response
        logger.LogTrace("Calling into userinfo response generator: {type}", responseGenerator.GetType().FullName);

        var response = await responseGenerator.ProcessAsync(validationResult);

        logger.LogDebug("End userinfo request");

        return new UserInfoResult(response);
    }
    
    private static IEndpointResult Error(string error, string? description = null)
    {
        return new ProtectedResourceErrorResult(error, description);
    }
}