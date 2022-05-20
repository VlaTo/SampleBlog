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
using System.Collections.Specialized;
using System.Net;

namespace SampleBlog.IdentityServer.Endpoints;

internal class AuthorizeEndpoint : AuthorizeEndpointBase
{
    public AuthorizeEndpoint(
        IEventService events,
        ILogger<AuthorizeEndpoint> logger,
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
        using var activity = Tracing.ActivitySource.StartActivity(Constants.EndpointNames.Authorize + "Endpoint");
        // todo: add complete url?

        Logger.LogDebug("Start authorize request");

        NameValueCollection values;

        if (HttpMethods.IsGet(context.Request.Method))
        {
            values = context.Request.Query.AsNameValueCollection();
        }
        else if (HttpMethods.IsPost(context.Request.Method))
        {
            if (!context.Request.HasApplicationFormContentType())
            {
                return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
            }

            values = context.Request.Form.AsNameValueCollection();
        }
        else
        {
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        var user = await UserSession.GetUserAsync();
        var result = await ProcessAuthorizeRequestAsync(values, user);

        Logger.LogTrace("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");

        return result;
    }
}