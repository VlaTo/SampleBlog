using Microsoft.AspNetCore.Authorization;

namespace SampleBlog.Web.Shared.Authorization;

public sealed class PermissionAuthorizationRequirement : AuthorizationHandler<PermissionAuthorizationRequirement>, IAuthorizationRequirement
{
    private const string DefaultPermissionClaimType = "Permission";

    private readonly string[] permissions;

    public string PermissionClaimType
    {
        get;
        set;
    }

    public PermissionAuthorizationRequirement(string permission)
        : this(new[] { permission })
    {
    }

    public PermissionAuthorizationRequirement(params string[] permissions)
    {
        this.permissions = new string[permissions.Length];
        Array.Copy(permissions, this.permissions, permissions.Length);
        PermissionClaimType = DefaultPermissionClaimType;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
    {
        foreach (var claim in context.User.Claims)
        {
            if (false == String.Equals(claim.Type, PermissionClaimType))
            {
                continue;
            }

            if (permissions.Contains(claim.Value))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
