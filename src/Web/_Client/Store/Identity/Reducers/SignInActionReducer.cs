using Fluxor;
using SampleBlog.Web.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Client.Store.Identity.Reducers;

public sealed class SignInActionReducer : Reducer<IdentityState, SignInAction>
{
    public override IdentityState Reduce(IdentityState state, SignInAction action)
    {
        return new IdentityState(ModelState.Loading, state.Principal);
    }
}