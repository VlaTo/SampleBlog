using Fluxor;
using SampleBlog.Web.Client.Store.OriginalMenu.Actions;

namespace SampleBlog.Web.Client.Store.OriginalMenu.Reducers;

public sealed class NoMenuActionReducer : Reducer<OriginalMenuState, NoOriginalMenuAction>
{
    public override OriginalMenuState Reduce(OriginalMenuState state, NoOriginalMenuAction action)
    {
        return new OriginalMenuState(ModelState.Failed("no menu"), action.DateTime);
    }
}