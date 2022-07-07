using Fluxor;
using SampleBlog.Web.Client.Store.Menu.Actions;

namespace SampleBlog.Web.Client.Store.Menu.Reducers;

public sealed class NoMenuActionReducer : Reducer<MenuState, NoMenuAction>
{
    public override MenuState Reduce(MenuState state, NoMenuAction action)
    {
        return new MenuState(ModelState.Failed("no menu"), action.DateTime);
    }
}