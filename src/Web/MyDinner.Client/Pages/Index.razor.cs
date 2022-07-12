using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using SampleBlog.Web.Client.Core;
using SampleBlog.Web.Client.Core.Services;
using SampleBlog.Web.Client.Shared;
using SampleBlog.Web.Client.Store;
using SampleBlog.Web.Client.Store.Menu;
using SampleBlog.Web.Client.Store.Menu.Actions;
using SampleBlog.Web.Client.Store.Order.Actions;
using SampleBlog.Web.Shared.Models.Menu;
using System.Windows.Input;
using SampleBlog.Web.Client.Store.OriginalMenu;
using SampleBlog.Web.Client.Store.OriginalMenu.Actions;

namespace SampleBlog.Web.Client.Pages;

public partial class Index
{
    private MudChip? chip;

    [Inject]
    public IState<OriginalMenuState> State
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

    [Inject]
    public IDialogService DialogService
    {
        get;
        set;
    }

    public bool IsLoading => ModelState.Loading == State.Value.State;

    public TableGroupDefinition<Dish> ProductGroups
    {
        get;
    }

    public Dish[] Dishes => State.Value?.Dishes ?? Array.Empty<Dish>();

    public MudChip Chip
    {
        get => chip!;
        set
        {
            chip = value;

            var text = chip?.Text ?? "(no chip)";
            Console.WriteLine($"Selected chip: {text}");
        }
    }

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

    public MudTheme Theme
    {
        get;
    }

    public Index()
    {
        IncrementDish = new DelegateCommand<Dish>(DoIncrementDish);
        DecrementDish = new DelegateCommand<Dish>(DoDecrementDish);
        RemoveDish = new DelegateCommand<Dish>(DoRemoveDish);
        ProductGroups = new TableGroupDefinition<Dish>
        {
            GroupName = "Group",
            Indentation = false,
            Expandable = false,
            Selector = dish => dish.GroupName
        };
        Theme = new MudTheme();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var dateTime = CurrentDateTimeProvider.CurrentDateTime;

        Dispatcher.Dispatch(new GetOriginalMenuAction(dateTime));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            ;
        }
    }

    private void DoIncrementDish(Dish entry)
    {
        Dispatcher.Dispatch(new IncrementDishAction(entry, 1));
    }

    private void DoDecrementDish(Dish entry)
    {
        Dispatcher.Dispatch(new DecrementDishAction(entry, 1));
    }

    private void DoRemoveDish(Dish entry)
    {
        Dispatcher.Dispatch(new RemoveDishFromOrderAction(entry));
    }

    private void OnDishClick(MouseEventArgs e)
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true
        };

        DialogService.Show<DishPreviewDialog>("Dish", options);
    }
}