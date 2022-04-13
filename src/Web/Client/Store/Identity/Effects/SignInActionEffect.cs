using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using SampleBlog.Web.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Client.Store.Identity.Effects;

public sealed class SignInActionEffect : Effect<SignInAction>
{
    private readonly AuthenticationStateProvider authenticationProvider;

    public SignInActionEffect(AuthenticationStateProvider authenticationProvider)
    {
        this.authenticationProvider = authenticationProvider;
    }

    public override async Task HandleAsync(SignInAction action, IDispatcher dispatcher)
    {
        try
        {
            var authenticationState = await authenticationProvider.GetAuthenticationStateAsync();
            dispatcher.Dispatch(new UserAuthenticatedAction(authenticationState.User));
        }
        catch (Exception exception)
        {
            dispatcher.Dispatch(new AuthenticationFailedAction(exception));
        }
    }
}