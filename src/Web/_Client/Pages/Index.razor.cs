using Fluxor;
using Microsoft.AspNetCore.Components;
using SampleBlog.Web.Client.Store.Identity;
using SampleBlog.Web.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Client.Pages;

public partial class Index
{
    [Inject]
    public IState<IdentityState> State
    {
        get;
        set;
    }

    [Inject]
    public IDispatcher Dispatcher
    {
        get;
        set;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new SignInAction());
    }
}