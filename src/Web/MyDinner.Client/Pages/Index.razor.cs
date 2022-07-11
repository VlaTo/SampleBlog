using System.Globalization;
using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SampleBlog.Web.Client.Core;
using SampleBlog.Web.Client.Core.Services;
using SampleBlog.Web.Client.Store;
using SampleBlog.Web.Client.Store.Menu;
using SampleBlog.Web.Client.Store.Menu.Actions;
using SampleBlog.Web.Client.Store.Order.Actions;
using SampleBlog.Web.Shared.Models.Menu;
using System.Windows.Input;
using SampleBlog.Core.Domain.Entities;

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

    public ICommand IncrementDish
    {
        get;
    }

    public ICommand DecrementDish
    {
        get;
    }

    public ICommand RemoveDish
    {
        get;
    }

    public Index()
    {
        IncrementDish = new DelegateCommand<DishEntry>(DoIncrementDish);
        DecrementDish = new DelegateCommand<DishEntry>(DoDecrementDish);
        RemoveDish = new DelegateCommand<DishEntry>(DoRemoveDish);
        ProductGroups = new TableGroupDefinition<DishEntry>
        {
            GroupName = "Group",
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

    private void DoIncrementDish(DishEntry entry)
    {
        Dispatcher.Dispatch(new IncrementDishAction(entry, 1));
    }

    private void DoDecrementDish(DishEntry entry)
    {
        Dispatcher.Dispatch(new DecrementDishAction(entry, 1));
    }

    private void DoRemoveDish(DishEntry entry)
    {
        Dispatcher.Dispatch(new RemoveDishFromOrderAction(entry));
    }
}