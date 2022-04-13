using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace SampleBlog.IdentityServer.Hosting.FederatedSignOut;

internal sealed class AuthenticationRequestSignInHandlerWrapper : AuthenticationRequestSignOutHandlerWrapper, IAuthenticationSignInHandler
{
    private readonly IAuthenticationSignInHandler handler;

    public AuthenticationRequestSignInHandlerWrapper(
        IAuthenticationSignInHandler handler,
        IHttpContextAccessor httpContextAccessor)
        : base(handler, httpContextAccessor)
    {
        this.handler = handler;
    }

    public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        return handler.SignInAsync(user, properties);
    }
}