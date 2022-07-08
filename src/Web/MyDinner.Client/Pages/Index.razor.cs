using System.Diagnostics;
using System.Windows.Input;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SampleBlog.Web.Client.Core;
using SampleBlog.Web.Client.Core.Services;
using SampleBlog.Web.Client.Store;
using SampleBlog.Web.Client.Store.Menu;
using SampleBlog.Web.Client.Store.Menu.Actions;
using SampleBlog.Web.Client.Store.Order.Actions;
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

    public TableGroupDefinition<DishEntry> ProductGroups
    {
        get;
    }

    public DishEntry[] Dishes => State.Value?.Dishes ?? Array.Empty<DishEntry>();

    public ICommand AddToMenu
    {
        get;
    }

    public Index()
    {
        AddToMenu = new DelegateCommand<DishEntry>(DoAddToMenu);
        ProductGroups = new TableGroupDefinition<DishEntry>
        {
            GroupName = nameof(DishEntry.GroupName),
            Indentation = false,
            Expandable = false,
            Selector = dish => dish.GroupName
        };
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var dateTime = CurrentDateTimeProvider.CurrentDateTime;

        Dispatcher.Dispatch(new GetMenuAction(dateTime));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            ;
        }
    }

    private void DoAddToMenu(DishEntry entry)
    {
        Dispatcher.Dispatch(new AddToOrderAction(entry));
    }
}