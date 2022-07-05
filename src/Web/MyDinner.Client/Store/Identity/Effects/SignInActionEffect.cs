using System.Security.Claims;
using System.Text;
using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using SampleBlog.Web.Application.MyDinner.Client.Extensions;
using SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Effects;

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
            LogUser(authenticationState.User);
            dispatcher.Dispatch(new UserAuthenticatedAction(authenticationState.User));
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            dispatcher.Dispatch(new AuthenticationFailedAction(exception));
        }
    }

    private static void LogUser(ClaimsPrincipal principal)
    {
        var builder = new StringBuilder();

        builder.AppendIf($"Has identity: {principal.Identity}", null != principal.Identity);
        builder.AppendIf($"Is Authenticated: {principal.Identity.Name}", principal.Identity.IsAuthenticated);

        foreach (var claim in principal.Claims)
        {
            builder.AppendLine($"   [{claim.Type}] = {claim.Value}");
        }

        Console.WriteLine($"User: {builder}");
    }
}