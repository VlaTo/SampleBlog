using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Endpoints.Results;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.ResponseHandling;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Stores;
using System.Net;

namespace SampleBlog.IdentityServer.Endpoints;

internal class AuthorizeCallbackEndpoint : AuthorizeEndpointBase
{
    public AuthorizeCallbackEndpoint(
        IEventService events,
        ILogger<AuthorizeCallbackEndpoint> logger,
        IdentityServerOptions options,
        IAuthorizeRequestValidator validator,
        IAuthorizeInteractionResponseGenerator interactionGenerator,
        IAuthorizeResponseGenerator authorizeResponseGenerator,
        IUserSession userSession,
        IConsentMessageStore consentResponseStore,
        IAuthorizationParametersMessageStore? authorizationParametersMessageStore = null)
        : base(
            events,
            logger,
            options,
            validator,
            interactionGenerator,
            authorizeResponseGenerator,
            userSession,
            consentResponseStore,
            authorizationParametersMessageStore)
    {
    }

    public override async Task<IEndpointResult?> ProcessAsync(HttpContext context)
    {
        using var activity = Tracing.ActivitySource.StartActivity(Constants.EndpointNames.Authorize + "CallbackEndpoint");

        if (false == HttpMethods.IsGet(context.Request.Method))
        {
            Logger.LogWarning("Invalid HTTP method for authorize endpoint.");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        Logger.LogDebug("Start authorize callback request");

        var parameters = context.Request.Query.AsNameValueCollection();
        var user = await UserSession.GetUserAsync();

        var result = await ProcessAuthorizeRequestAsync(parameters, user, true);

        Logger.LogTrace("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

        return result;
    }
}