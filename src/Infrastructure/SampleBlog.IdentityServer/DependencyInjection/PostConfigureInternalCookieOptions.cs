using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleBlog.IdentityServer.DependencyInjection.Options;

namespace SampleBlog.IdentityServer.DependencyInjection;

internal class PostConfigureInternalCookieOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly IdentityServerOptions serverOptions;
    private readonly IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions;
    private readonly ILogger logger;

    public PostConfigureInternalCookieOptions(
        IdentityServerOptions serverOptions,
        IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions,
        ILoggerFactory loggerFactory)
    {
        this.serverOptions = serverOptions;
        this.authOptions = authOptions;

        logger = loggerFactory.CreateLogger("SampleBlog.IdentityServer.Startup");
    }

    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        var scheme = serverOptions.Authentication.CookieAuthenticationScheme ??
                     authOptions.Value.DefaultAuthenticateScheme ??
                     authOptions.Value.DefaultScheme;

        if (name == scheme)
        {
            serverOptions.UserInteraction.LoginUrl = serverOptions.UserInteraction.LoginUrl ?? options.LoginPath;
            serverOptions.UserInteraction.LoginReturnUrlParameter = serverOptions.UserInteraction.LoginReturnUrlParameter ?? options.ReturnUrlParameter;
            serverOptions.UserInteraction.LogoutUrl = serverOptions.UserInteraction.LogoutUrl ?? options.LogoutPath;

            logger.LogDebug("Login Url: {url}", serverOptions.UserInteraction.LoginUrl);
            logger.LogDebug("Login Return Url Parameter: {param}", serverOptions.UserInteraction.LoginReturnUrlParameter);
            logger.LogDebug("Logout Url: {url}", serverOptions.UserInteraction.LogoutUrl);

            logger.LogDebug("ConsentUrl Url: {url}", serverOptions.UserInteraction.ConsentUrl);
            logger.LogDebug("Consent Return Url Parameter: {param}", serverOptions.UserInteraction.ConsentReturnUrlParameter);

            logger.LogDebug("Error Url: {url}", serverOptions.UserInteraction.ErrorUrl);
            logger.LogDebug("Error Id Parameter: {param}", serverOptions.UserInteraction.ErrorIdParameter);
        }
    }
}