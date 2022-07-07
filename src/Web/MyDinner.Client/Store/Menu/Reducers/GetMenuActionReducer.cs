using Fluxor;
using SampleBlog.Web.Client.Store.Menu.Actions;

namespace SampleBlog.Web.Client.Store.Menu.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class GetMenuActionReducer : Reducer<MenuState, GetMenuAction>
{
    public override MenuState Reduce(MenuState state, GetMenuAction action)
    {
        return new MenuState(ModelState.Loading, state.DateTime, state.Dishes);
    }
}