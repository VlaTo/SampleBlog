using Fluxor;
using SampleBlog.Web.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Client.Store.Identity.Reducers;

public class UserAuthenticatedActionReducer : Reducer<IdentityState, UserAuthenticatedAction>
{
    public override IdentityState Reduce(IdentityState state, UserAuthenticatedAction action)
    {
        return new IdentityState(ModelState.Success, action.Principal);
    }
}