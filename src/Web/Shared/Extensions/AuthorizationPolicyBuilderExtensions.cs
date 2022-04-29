using Microsoft.AspNetCore.Authorization;
using SampleBlog.Web.Shared.Authorization;

namespace SampleBlog.Web.Shared.Extensions;

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder policyBuilder, string permission)
    {
        policyBuilder.RequireAuthenticatedUser();
        return policyBuilder;
    }

    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder policyBuilder, params string[] otherPermissions)
    {
        var builder = policyBuilder.RequireAuthenticatedUser();
        builder.Requirements.Add(new PermissionAuthorizationRequirement());
        return policyBuilder;
    }
}