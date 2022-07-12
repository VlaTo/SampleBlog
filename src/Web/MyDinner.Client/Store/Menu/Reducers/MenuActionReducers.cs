using Fluxor;
using SampleBlog.Web.Client.Store.Menu.Actions;
using SampleBlog.Web.Shared.Models.Menu;
using System.Collections.Immutable;

namespace SampleBlog.Web.Client.Store.Menu.Reducers;

/// <summary>
/// 
/// </summary>
// ReSharper disable once UnusedMember.Global
public sealed class GetOriginalMenuActionReducer : Reducer<MenuState, GetOriginalMenuAction>
{
    public override MenuState Reduce(MenuState state, GetOriginalMenuAction action)
    {
        // new state
        return new MenuState(ModelState.Loading)
        {
            DateTime = state.DateTime,
            FoodCategories = state.FoodCategories,
            OriginalMenu = state.OriginalMenu,
            Menu = state.Menu,
            IsAvailable = state.IsAvailable,
            SelectedFoodCategory = null
        };
    }
}

/// <summary>
/// 
/// </summary>
// ReSharper disable once UnusedMember.Global
public sealed class OriginalMenuAcquiredActionReducer : Reducer<MenuState, OriginalMenuAcquiredAction>
{
    public override MenuState Reduce(MenuState state, OriginalMenuAcquiredAction action)
    {
        var originalMenu = action.OriginalMenu.ToImmutableList();
        // new state
        return new MenuState(ModelState.Success)
        {
            DateTime = action.DateTime,
            FoodCategories = BuildFoodCategories(action.OriginalMenu),
            OriginalMenu = originalMenu,
            IsAvailable = action.IsOpen,
            SelectedFoodCategory = null,
            Menu = originalMenu
        };
    }

    private static ImmutableArray<FoodCategory> BuildFoodCategories(IReadOnlyList<Dish> menu)
    {
        var foodCategories = new Dictionary<string, FoodCategory>();

        for (var index = 0; index < menu.Count; index++)
        {
            var foodCategory = menu[index].FoodCategory;

            if (null == foodCategory)
            {
                continue;
            }

            if (foodCategories.ContainsKey(foodCategory.Key))
            {
                continue;
            }

            foodCategories.Add(foodCategory.Key, foodCategory);
        }

        return foodCategories.Values.ToImmutableArray();
    }
}

/// <summary>
/// 
/// </summary>
// ReSharper disable once UnusedMember.Global
public sealed class NoOriginalMenuActionReducer : Reducer<MenuState, NoOriginalMenuAction>
{
    public override MenuState Reduce(MenuState state, NoOriginalMenuAction action)
    {
        // new state
        return new MenuState(ModelState.Failed(action.error ?? "(empty)"))
        {
            DateTime = action.DateTime,
            FoodCategories = ImmutableArray<FoodCategory>.Empty,
            OriginalMenu = ImmutableList<Dish>.Empty,
            Menu = ImmutableList<Dish>.Empty,
            SelectedFoodCategory = null,
            IsAvailable = false
        };
    }
}

/// <summary>
/// 
/// </summary>
// ReSharper disable once UnusedMember.Global
public sealed class FilterOriginalMenuActionReducer : Reducer<MenuState, FilterOriginalMenuAction>
{
    public override MenuState Reduce(MenuState state, FilterOriginalMenuAction action)
    {
        return new MenuState(state.State)
        {
            DateTime = state.DateTime,
            FoodCategories = state.FoodCategories,
            OriginalMenu = state.OriginalMenu,
            IsAvailable = state.IsAvailable,
            SelectedFoodCategory = FindFoodCategory(state.FoodCategories, action.FoodCategoryKey),
            Menu = FilterFoodCategory(state.OriginalMenu, action.FoodCategoryKey)
        };
    }

    private static ImmutableList<Dish> FilterFoodCategory(ImmutableList<Dish> originalMenu, string foodCategoryKey)
    {
        var menu = new List<Dish>();

        for (var index = 0; index < originalMenu.Count; index++)
        {
            if (false == String.Equals(originalMenu[index].FoodCategory?.Key, foodCategoryKey))
            {
                continue;
            }

            menu.Add(originalMenu[index]);
        }

        return menu.ToImmutableList();
    }

    private static FoodCategory? FindFoodCategory(ImmutableArray<FoodCategory> foodCategories, string foodCategoryKey)
    {
        for (var index = 0; index < foodCategories.Length; index++)
        {
            if (false == String.Equals(foodCategories[index].Key, foodCategoryKey))
            {
                continue;
            }

            return foodCategories[index];
        }

        return null;
    }
}

/// <summary>
/// 
/// </summary>
// ReSharper disable once UnusedMember.Global
public sealed class ResetOriginalMenuFilterActionReducer : Reducer<MenuState, ResetOriginalMenuFilterAction>
{
    public override MenuState Reduce(MenuState state, ResetOriginalMenuFilterAction action)
    {
        // new state
        return new MenuState(state.State)
        {
            DateTime = state.DateTime,
            FoodCategories = state.FoodCategories,
            OriginalMenu = state.OriginalMenu,
            IsAvailable = state.IsAvailable,
            SelectedFoodCategory = null,
            Menu = state.OriginalMenu
        };
    }
}