using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.Authorization;

namespace SampleBlog.Web.Client.Shared;

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
}