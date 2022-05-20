using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Core.Events;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;

namespace SampleBlog.IdentityServer.Hosting;

/// <summary>
/// IdentityServer middleware
/// </summary>
public class IdentityServerMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityServerMiddleware" /> class.
    /// </summary>
    /// <param name="next">The next.</param>
    /// <param name="logger">The logger.</param>
    public IdentityServerMiddleware(
        RequestDelegate next,
        ILogger<IdentityServerMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="router">The router.</param>
    /// <param name="userSession">The user session.</param>
    /// <param name="events">The event service.</param>
    /// <param name="issuerNameService">The issuer name service</param>
    /// <param name="sessionCoordinationService"></param>
    /// <returns></returns>
    public async Task Invoke(
        HttpContext context,
        IEndpointRouter router,
        IUserSession userSession,
        IEventService events,
        IIssuerNameService issuerNameService,
        ISessionCoordinationService sessionCoordinationService)
    {
        // this will check the authentication session and from it emit the check session
        // cookie needed from JS-based signout clients.
        await userSession.EnsureSessionIdCookieAsync();

        context.Response.OnStarting(async () =>
        {
            if (context.GetSignOutCalled())
            {
                logger.LogDebug("SignOutCalled set; processing post-signout session cleanup.");

                // this clears our session id cookie so JS clients can detect the user has signed out
                await userSession.RemoveSessionIdCookieAsync();

                var user = await userSession.GetUserAsync();

                if (null != user)
                {
                    var session = new UserSession
                    {
                        SubjectId = user.GetSubjectId(),
                        SessionId = await userSession.GetSessionIdAsync(),
                        DisplayName = user.GetDisplayName(),
                        ClientIds = (await userSession.GetClientListAsync()).ToList(),
                        Issuer = await issuerNameService.GetCurrentAsync()
                    };

                    await sessionCoordinationService.ProcessLogoutAsync(session);
                }
            }
        });

        try
        {
            var endpoint = router.Find(context);

            if (null != endpoint)
            {
                var endpointType = endpoint.GetType().FullName;

                using var activity = Tracing.BasicActivitySource.StartActivity("IdentityServerProtocolRequest");
                activity?.SetTag(Tracing.Properties.EndpointType, endpointType);

                //LicenseValidator.ValidateIssuer(await issuerNameService.GetCurrentAsync());

                logger.LogInformation("Invoking SampleBlog.IdentityServer endpoint: {endpointType} for {url}", endpointType, context.Request.Path.ToString());

                var result = await endpoint.ProcessAsync(context);

                logger.LogTrace("Invoking result: {type}", result.GetType().FullName);

                await result.ExecuteAsync(context);

                return;
            }
        }
        catch (Exception ex)
        {
            await events.RaiseAsync(new UnhandledExceptionEvent(ex));

            logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);

            throw;
        }

        await next.Invoke(context);
    }
}