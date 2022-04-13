using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SampleBlog.Web.Client.Services;

internal sealed class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    public ApiAuthenticationStateProvider()
    {
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test name"),
            new Claim(ClaimTypes.Actor, "testUser"),
            new Claim(ClaimTypes.Email, "test@test.org"),
            new Claim(ClaimTypes.Anonymous, "true", ClaimValueTypes.Boolean),
        };

        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(principal));
    }
}