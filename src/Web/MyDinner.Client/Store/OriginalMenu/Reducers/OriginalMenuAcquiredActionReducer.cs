using Fluxor;
using SampleBlog.Web.Client.Store.OriginalMenu.Actions;

namespace SampleBlog.Web.Client.Store.OriginalMenu.Reducers;

// ReSharper disable once UnusedMember.Global
public sealed class OriginalMenuAcquiredActionReducer : Reducer<OriginalMenuState, OriginalMenuAcquiredAction>
{
    public override OriginalMenuState Reduce(OriginalMenuState state, OriginalMenuAcquiredAction action)
    {
        return new OriginalMenuState(ModelState.Success, action.DateTime, action.Dishes);
    }
}