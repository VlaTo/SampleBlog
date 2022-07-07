using Fluxor;
using SampleBlog.Web.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Client.Store.Identity.Reducers;

public class AuthenticationFailedActionReducer : Reducer<IdentityState, AuthenticationFailedAction>
{
    public override IdentityState Reduce(IdentityState state, AuthenticationFailedAction action)
    {
        return new IdentityState(ModelState.Failed(action.Exception.Message), state.Principal);
    }
}