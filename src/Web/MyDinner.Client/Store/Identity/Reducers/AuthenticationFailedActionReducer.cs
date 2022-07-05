using Fluxor;
using SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Reducers;

public class AuthenticationFailedActionReducer : Reducer<IdentityState, AuthenticationFailedAction>
{
    public override IdentityState Reduce(IdentityState state, AuthenticationFailedAction action)
    {
        return new IdentityState(ModelState.Failed(action.Exception.Message), state.Principal);
    }
}