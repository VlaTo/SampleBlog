using Fluxor;
using SampleBlog.Web.Client.Store.Menu.Actions;

namespace SampleBlog.Web.Client.Store.Menu.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class MenuAcquiredActionReducer : Reducer<MenuState, MenuAcquiredAction>
{
    public override MenuState Reduce(MenuState state, MenuAcquiredAction action)
    {
        return new MenuState(ModelState.Success, action.DateTime, action.Dishes);
    }
}