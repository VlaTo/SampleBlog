using System.Collections.Immutable;
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

namespace SampleBlog.Web.Client.Pages;

public partial class Index
{
    private MudChip? selectedFoodCategoryChip;

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

    public ImmutableList<Dish> Dishes => State.Value.Menu;

    public ImmutableArray<FoodCategory> FoodCategories => State.Value.FoodCategories;

    public MudChip SelectedFoodCategoryChip
    {
        get => selectedFoodCategoryChip!;
        set
        {
            selectedFoodCategoryChip = value;

            if (null == selectedFoodCategoryChip)
            {
                return;
            }

            if (String.IsNullOrEmpty(selectedFoodCategoryChip.Text))
            {
                Dispatcher.Dispatch(new ResetOriginalMenuFilterAction());
                return;
            }

            Dispatcher.Dispatch(new FilterOriginalMenuAction(selectedFoodCategoryChip.Text));
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

    public ICommand OrderNow
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
        OrderNow = new DelegateCommand(DoOrderNow);
        ProductGroups = new TableGroupDefinition<Dish>
        {
            GroupName = "FoodCategories",
            Indentation = false,
            Expandable = false,
            Selector = dish => dish.FoodCategory?.Name
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

    private void DoOrderNow()
    {
        ;
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