using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace SampleBlog.IdentityServer.Hosting.FederatedSignOut;

internal class AuthenticationRequestSignOutHandlerWrapper : AuthenticationRequestHandlerWrapper, IAuthenticationSignOutHandler
{
    private readonly IAuthenticationSignOutHandler handler;

    public AuthenticationRequestSignOutHandlerWrapper(
        IAuthenticationSignOutHandler handler,
        IHttpContextAccessor httpContextAccessor)
        : base((IAuthenticationRequestHandler)handler, httpContextAccessor)
    {
        this.handler = handler;
    }

    public Task SignOutAsync(AuthenticationProperties? properties)
    {
        return handler.SignOutAsync(properties);
    }
}