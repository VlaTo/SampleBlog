using Fluxor;
using SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Actions;

namespace SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Reducers;

public sealed class SignInActionReducer : Reducer<IdentityState, SignInAction>
{
    public override IdentityState Reduce(IdentityState state, SignInAction action)
    {
        return new IdentityState(ModelState.Loading, state.Principal);
    }
}