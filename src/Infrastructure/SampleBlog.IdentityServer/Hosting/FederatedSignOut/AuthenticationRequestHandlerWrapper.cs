using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Extensions;
using System.Net;

namespace SampleBlog.IdentityServer.Hosting.FederatedSignOut;

internal class AuthenticationRequestHandlerWrapper : IAuthenticationRequestHandler
{
    private const string IframeHtml = "<iframe style='display:none' width='0' height='0' src='{0}'></iframe>";

    private readonly IAuthenticationRequestHandler handler;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger? logger;

    public AuthenticationRequestHandlerWrapper(IAuthenticationRequestHandler handler, IHttpContextAccessor httpContextAccessor)
    {
        this.handler = handler;
        this.httpContextAccessor = httpContextAccessor;

        //var factory = (ILoggerFactory?)httpContext?.RequestServices.GetService(typeof(ILoggerFactory));

        //logger = factory?.CreateLogger(GetType());
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        => handler.InitializeAsync(scheme, context);

    public Task<AuthenticateResult> AuthenticateAsync()
        => handler.AuthenticateAsync();

    public Task ChallengeAsync(AuthenticationProperties? properties)
        => handler.ChallengeAsync(properties);

    public Task ForbidAsync(AuthenticationProperties? properties)
        => handler.ForbidAsync(properties);

    public async Task<bool> HandleRequestAsync()
    {
        var result = await handler.HandleRequestAsync();

        if (result)
        {
            var context = httpContextAccessor.HttpContext;

            if (null != context && context.GetSignOutCalled() && (int)HttpStatusCode.OK == context.Response.StatusCode)
            {
                // given that this runs prior to the authentication middleware running
                // we need to explicitly trigger authentication so we can have our 
                // session service populated with the current user info
                await context.AuthenticateAsync();

                // now we can do our processing to render the iframe (if needed)
                await ProcessFederatedSignOutRequestAsync(context);
            }
        }

        return result;
    }

    private async Task ProcessFederatedSignOutRequestAsync(HttpContext context)
    {
        //_logger?.LogDebug("Processing federated signout");

        var iframeUrl = await context.GetIdentityServerSignOutFrameCallbackUrlAsync();

        if (null != iframeUrl)
        {
            //_logger?.LogDebug("Rendering signout callback iframe");
            await RenderResponseAsync(context, iframeUrl);
        }
        else
        {
            //_logger?.LogDebug("No signout callback iframe to render");
        }
    }

    private static async Task RenderResponseAsync(HttpContext context, string iframeUrl)
    {
        context.Response.SetNoCache();

        if (context.Response.Body.CanWrite)
        {
            var iframe = string.Format(IframeHtml, iframeUrl);
            await context.Response.WriteHtmlAsync(iframe);
        }
    }
}