using System.Security.Claims;

namespace SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Actions;

public sealed class UserAuthenticatedAction
{
    public ClaimsPrincipal Principal
    {
        get;
    }

    public UserAuthenticatedAction(ClaimsPrincipal principal)
    {
        Principal = principal;
    }
}