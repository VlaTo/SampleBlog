using System.Diagnostics;
using System.Windows.Input;
using Fluxor;
using Microsoft.AspNetCore.Components;
using SampleBlog.Web.Client.Core;
using SampleBlog.Web.Client.Core.Services;
using SampleBlog.Web.Client.Store;
using SampleBlog.Web.Client.Store.Menu;
using SampleBlog.Web.Client.Store.Menu.Actions;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Pages;

public partial class Index
{
    [Inject]
    public IState<MenuState> State
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

    [Inject]
    public ICurrentDateTimeProvider CurrentDateTimeProvider
    {
        get;
        set;
    }

    public bool IsLoading => ModelState.Loading == State.Value.State;

    public DishEntry[]? Dishes => State.Value?.Dishes ?? Array.Empty<DishEntry>();

    public ICommand AddToMenu
    {
        get;
    }

    public Index()
    {
        AddToMenu = new DelegateCommand<DishEntry>(DoAddToMenu);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var dateTime = CurrentDateTimeProvider.CurrentDateTime;

        Dispatcher.Dispatch(new GetMenuAction(dateTime));
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }

    private void DoAddToMenu(DishEntry entry)
    {
        Debugger.Break();
    }
}