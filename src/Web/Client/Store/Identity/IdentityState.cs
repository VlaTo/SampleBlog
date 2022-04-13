using System.Security.Claims;
using Fluxor;

namespace SampleBlog.Web.Client.Store.Identity;

public sealed class IdentityStateFeature : Feature<IdentityState>
{
    public override string GetName() => nameof(IdentityState);

    protected override IdentityState GetInitialState() => new(ModelState.None);
}

public class IdentityState : StateBase
{
    public ClaimsPrincipal Principal
    {
        get;
    }

    public IdentityState(ModelState state)
        : base(state)
    {
        Principal = new ClaimsPrincipal();
    }

    public IdentityState(ModelState state, ClaimsPrincipal principal)
        : base(state)
    {
        Principal = principal;
    }
}