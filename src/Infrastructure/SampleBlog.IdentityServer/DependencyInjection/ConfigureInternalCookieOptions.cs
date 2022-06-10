using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.DependencyInjection;

internal class ConfigureInternalCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    private readonly IdentityServerOptions serverOptions;

    public ConfigureInternalCookieOptions(IdentityServerOptions serverOptions)
    {
        this.serverOptions = serverOptions;
    }

    public void Configure(CookieAuthenticationOptions options)
    {
    }

    public void Configure(string name, CookieAuthenticationOptions options)
    {
        // actual name: Identity.Application
        // default name: idsrv
        // external name: idsrv.external
        // cookie-name: AspNetCore.Identity.Application

        if (String.Equals(IdentityServerConstants.DefaultCookieAuthenticationScheme, name))
        {
            options.SlidingExpiration = serverOptions.Authentication.CookieSlidingExpiration;
            options.ExpireTimeSpan = serverOptions.Authentication.CookieLifetime;
            options.Cookie.Name = IdentityServerConstants.DefaultCookieAuthenticationScheme;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = serverOptions.Authentication.CookieSameSiteMode;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.LoginPath = ExtractLocalUrl(serverOptions.UserInteraction.LoginUrl);
            options.LogoutPath = ExtractLocalUrl(serverOptions.UserInteraction.LogoutUrl);

            if (serverOptions.UserInteraction.LoginReturnUrlParameter != null)
            {
                options.ReturnUrlParameter = serverOptions.UserInteraction.LoginReturnUrlParameter;
            }
        }

        if (String.Equals(IdentityServerConstants.ExternalCookieAuthenticationScheme, name))
        {
            options.Cookie.Name = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            options.Cookie.IsEssential = true;
            // https://github.com/IdentityServer/IdentityServer4/issues/2595
            // need to set None because iOS 12 safari considers the POST back to the client from the 
            // IdP as not safe, so cookies issued from response (with lax) then should not be honored.
            // so we need to make those cookies issued without same-site, thus the browser will
            // hold onto them and send on the next redirect to the callback page.
            // see: https://brockallen.com/2019/01/11/same-site-cookies-asp-net-core-and-external-authentication-providers/
            options.Cookie.SameSite = serverOptions.Authentication.CookieSameSiteMode;
        }

        options.Cookie.SameSite = serverOptions.Authentication.CookieSameSiteMode;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }

    private static string? ExtractLocalUrl(string? url)
    {
        if (null != url && url.IsLocalUrl())
        {
            if (url.StartsWith("~/"))
            {
                url = url[1..];
            }

            return url;
        }

        return null;
    }
}