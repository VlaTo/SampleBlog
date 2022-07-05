using Fluxor;
using SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Reducers;

public class UserAuthenticatedActionReducer : Reducer<IdentityState, UserAuthenticatedAction>
{
    public override IdentityState Reduce(IdentityState state, UserAuthenticatedAction action)
    {
        return new IdentityState(ModelState.Success, action.Principal);
    }
}