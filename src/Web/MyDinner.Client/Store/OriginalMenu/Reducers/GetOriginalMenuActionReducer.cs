using Fluxor;
using SampleBlog.Web.Client.Store.OriginalMenu.Actions;

namespace SampleBlog.Web.Client.Store.OriginalMenu.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class GetOriginalMenuActionReducer : Reducer<OriginalMenuState, GetOriginalMenuAction>
{
    public override OriginalMenuState Reduce(OriginalMenuState state, GetOriginalMenuAction action)
    {
        return new OriginalMenuState(ModelState.Loading, state.DateTime, state.Dishes);
    }
}