using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SampleBlog.IdentityServer.DependencyInjection.Options;

namespace SampleBlog.Identity.Authorization.Configuration;

internal class AspNetConventionsConfigureOptions : IConfigureOptions<IdentityServerOptions>
{
    public void Configure(IdentityServerOptions options)
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.Authentication.CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;
        options.UserInteraction.ErrorUrl = "/Home";
    }
}