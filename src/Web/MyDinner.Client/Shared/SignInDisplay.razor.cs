using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace SampleBlog.Web.Application.MyDinner.Client.Shared;

public partial class SignInDisplay
{
    [Inject]
    public NavigationManager Navigation
    {
        get;
        set;
    }

    [Inject]
    public SignOutSessionStateManager SignOutManager
    {
        get;
        set;
    }

    [CascadingParameter]
    public Task<AuthenticationState> AuthenticationStateTask
    {
        get;
        set;
    }

    private async Task BeginSignOut(MouseEventArgs args)
    {
        await SignOutManager.SetSignOutState();
        Navigation.NavigateTo("authentication/logout");
    }

    private async Task BeginSignIn(MouseEventArgs args)
    {
        var state = await AuthenticationStateTask;
        //state.User.
    }
}