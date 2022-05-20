using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SampleBlog.Identity.Authorization.Configuration;
using SampleBlog.Identity.Authorization.Extensions;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Validation;
using System.Collections.Specialized;
using System.Net;

namespace SampleBlog.Identity.Authorization.Core;

internal class AutoRedirectEndSessionEndpoint : IEndpointHandler
{
    private readonly ILogger logger;
    private readonly IUserSession session;
    private readonly IOptions<IdentityServerOptions> identityServerOptions;
    private readonly IEndSessionRequestValidator validator;

    public AutoRedirectEndSessionEndpoint(
        IEndSessionRequestValidator validator,
        IOptions<IdentityServerOptions> identityServerOptions,
        IUserSession session,
        ILogger<AutoRedirectEndSessionEndpoint> logger)
    {
        this.logger = logger;
        this.session = session;
        this.identityServerOptions = identityServerOptions;
        this.validator = validator;
    }

    public async Task<IEndpointResult?> ProcessAsync(HttpContext ctx)
    {
        var validateRequest = ValidateRequest(ctx.Request);

        if (null != validateRequest)
        {
            return validateRequest;
        }

        var parameters = await GetParametersAsync(ctx.Request);
        var user = await session.GetUserAsync();
        var result = await validator.ValidateAsync(parameters, user);
        
        if (result.IsError)
        {
            logger.LogError(LoggerEventIds.EndingSessionFailed, "Error ending session {Error}", result.Error);
            return new RedirectResult(identityServerOptions.Value.UserInteraction.ErrorUrl);
        }

        var client = result.ValidatedRequest?.Client;

        if (client != null && client.Properties.TryGetValue(ApplicationProfilesPropertyNames.Profile, out _))
        {
            var signInScheme = identityServerOptions.Value.Authentication.CookieAuthenticationScheme;

            if (null != signInScheme)
            {
                await ctx.SignOutAsync(signInScheme);
            }
            else
            {
                await ctx.SignOutAsync();
            }

            var postLogOutUri = result.ValidatedRequest.PostLogOutUri;

            if (null != result.ValidatedRequest.State)
            {
                postLogOutUri = QueryHelpers.AddQueryString(postLogOutUri, OpenIdConnectParameterNames.State, result.ValidatedRequest.State);
            }

            return new RedirectResult(postLogOutUri);
        }

        return new RedirectResult(identityServerOptions.Value.UserInteraction.LogoutUrl);
    }
    
    private static async Task<NameValueCollection> GetParametersAsync(HttpRequest request)
    {
        if (HttpMethods.IsGet(request.Method))
        {
            return request.Query.AsNameValueCollection();
        }
        else
        {
            var form = await request.ReadFormAsync();
            return form.AsNameValueCollection();
        }
    }

    private static IEndpointResult? ValidateRequest(HttpRequest request)
    {
        const string formUlrEncoded = "application/x-www-form-urlencoded";

        if (!HttpMethods.IsPost(request.Method) && !HttpMethods.IsGet(request.Method))
        {
            return new IdentityServer.Endpoints.Results.StatusCodeResult(HttpStatusCode.BadRequest);
        }

        if (HttpMethods.IsPost(request.Method) && false == String.Equals(request.ContentType, formUlrEncoded, StringComparison.OrdinalIgnoreCase))
        {
            return new IdentityServer.Endpoints.Results.StatusCodeResult(HttpStatusCode.BadRequest);
        }

        return null;
    }

    #region RedirectResult

    internal class RedirectResult : IEndpointResult
    {
        public string Url
        {
            get;
        }

        public RedirectResult(string url)
        {
            Url = url;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            context.Response.Redirect(Url);
            return Task.CompletedTask;
        }
    }

    #endregion
}