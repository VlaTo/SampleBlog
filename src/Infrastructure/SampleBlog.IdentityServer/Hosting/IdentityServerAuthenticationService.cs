using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;
using System.Security.Claims;

namespace SampleBlog.IdentityServer.Hosting;

internal class IdentityServerAuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationService service;
    private readonly IAuthenticationSchemeProvider schemes;
    private readonly ISystemClock clock;
    private readonly IUserSession session;
    private readonly ILogger<IdentityServerAuthenticationService> logger;

    public IdentityServerAuthenticationService(
        Decorator<IAuthenticationService> decorator,
        IAuthenticationSchemeProvider schemes,
        ISystemClock clock,
        IUserSession session,
        ILogger<IdentityServerAuthenticationService> logger)
    {
        service = decorator.Instance;
        this.schemes = schemes;
        this.clock = clock;
        this.session = session;
        this.logger = logger;
    }

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        => service.AuthenticateAsync(context, scheme);

    public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        => service.ChallengeAsync(context, scheme, properties);

    public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        => service.ForbidAsync(context, scheme, properties);

    public async Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
    {
        var defaultScheme = await schemes.GetDefaultSignInSchemeAsync();
        var cookieScheme = await context.GetCookieAuthenticationSchemeAsync();

        if ((null == scheme && cookieScheme == defaultScheme?.Name) || scheme == cookieScheme)
        {
            AssertRequiredClaims(principal);
            AugmentMissingClaims(principal, clock.UtcNow.UtcDateTime);

            properties ??= new AuthenticationProperties();
            await session.CreateSessionIdAsync(principal, properties);
        }

        await service.SignInAsync(context, scheme, principal, properties);
    }

    public async Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        var defaultScheme = await schemes.GetDefaultSignOutSchemeAsync();
        var cookieScheme = await context.GetCookieAuthenticationSchemeAsync();

        if ((scheme == null && defaultScheme?.Name == cookieScheme) || scheme == cookieScheme)
        {
            // this sets a flag used by middleware to do post-signout work.
            context.SetSignOutCalled();
        }

        await service.SignOutAsync(context, scheme, properties);
    }

    private static void AssertRequiredClaims(ClaimsPrincipal principal)
    {
        // for now, we don't allow more than one identity in the principal/cookie
        if (1 != principal.Identities.Count())
        {
            throw new InvalidOperationException("only a single identity supported");
        }

        if (principal.FindFirst(JwtClaimTypes.Subject) == null) throw new InvalidOperationException("sub claim is missing");
    }

    private void AugmentMissingClaims(ClaimsPrincipal principal, DateTime authTime)
    {
        var identity = principal.Identities.First();

        // ASP.NET Identity issues this claim type and uses the authentication middleware name
        // such as "Google" for the value. this code is trying to correct/convert that for
        // our scenario. IOW, we take their old AuthenticationMethod value of "Google"
        // and issue it as the idp claim. we then also issue a amr with "external"
        var amr = identity.FindFirst(ClaimTypes.AuthenticationMethod);

        if (null != amr &&
            null == identity.FindFirst(JwtClaimTypes.IdentityProvider) &&
            null == identity.FindFirst(JwtClaimTypes.AuthenticationMethod))
        {
            logger.LogDebug("Removing amr claim with value: {value}", amr.Value);
            identity.RemoveClaim(amr);

            logger.LogDebug("Adding idp claim with value: {value}", amr.Value);
            identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, amr.Value));

            logger.LogDebug("Adding amr claim with value: {value}", Constants.ExternalAuthenticationMethod);
            identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, Constants.ExternalAuthenticationMethod));
        }

        if (null == identity.FindFirst(JwtClaimTypes.IdentityProvider))
        {
            logger.LogDebug("Adding idp claim with value: {value}", IdentityServerConstants.LocalIdentityProvider);
            identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, IdentityServerConstants.LocalIdentityProvider));
        }

        if (null == identity.FindFirst(JwtClaimTypes.AuthenticationMethod))
        {
            if (IdentityServerConstants.LocalIdentityProvider == identity.FindFirst(JwtClaimTypes.IdentityProvider)?.Value)
            {
                logger.LogDebug("Adding amr claim with value: {value}", OidcConstants.AuthenticationMethods.Password);
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, OidcConstants.AuthenticationMethods.Password));
            }
            else
            {
                logger.LogDebug("Adding amr claim with value: {value}", Constants.ExternalAuthenticationMethod);
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, Constants.ExternalAuthenticationMethod));
            }
        }

        if (null == identity.FindFirst(JwtClaimTypes.AuthenticationTime))
        {
            var time = new DateTimeOffset(authTime).ToUnixTimeSeconds().ToString();

            logger.LogDebug("Adding auth_time claim with value: {value}", time);
            identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationTime, time, ClaimValueTypes.Integer64));
        }
    }
}