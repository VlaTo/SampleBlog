using System.Collections.Immutable;
using Fluxor;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Menu;

// ReSharper disable once UnusedMember.Global
public sealed class MenuStateFeature : Feature<MenuState>
{
    public override string GetName() => nameof(MenuState);

    protected override MenuState GetInitialState() => new(ModelState.None)
    {
        DateTime = DateTime.Now,
        FoodCategories = ImmutableArray<FoodCategory>.Empty,
        SelectedFoodCategory = null,
        OriginalMenu = ImmutableList<Dish>.Empty,
        Menu = ImmutableList<Dish>.Empty,
        IsAvailable = false
    };
}

/// <summary>
/// The original menu store class.
/// </summary>
public sealed class MenuState : StateBase
{
    public DateTime? DateTime
    {
        get;
        init;
    }

    public ImmutableArray<FoodCategory> FoodCategories
    {
        get;
        init;
    }

    public FoodCategory? SelectedFoodCategory
    {
        get;
        init;
    }

    public ImmutableList<Dish> OriginalMenu
    {
        get;
        init;
    }

    public ImmutableList<Dish> Menu
    {
        get;
        init;
    }

    public bool IsAvailable
    {
        get;
        init;
    }

    public MenuState(ModelState state)
        : base(state)
    {
        FoodCategories = ImmutableArray<FoodCategory>.Empty;
        OriginalMenu = ImmutableList<Dish>.Empty;
        Menu = ImmutableList<Dish>.Empty;
    }
}