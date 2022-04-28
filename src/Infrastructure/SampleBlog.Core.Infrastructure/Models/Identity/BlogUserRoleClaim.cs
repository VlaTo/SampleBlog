using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Infrastructure.Models.Identity;

public class BlogUserRoleClaim : IdentityRoleClaim<string>, IAuditableEntity<int>
{
    public DateTime Created
    {
        get;
        set;
    }

    public DateTime Modified
    {
        get;
        set;
    }

    public string? Description
    {
        get;
        set;
    }

    public string? Group
    {
        get;
        set;
    }

    public virtual BlogUserRole Role
    {
        get;
        set;
    }

    public BlogUserRoleClaim()
        : base()
    {
    }

    public BlogUserRoleClaim(string? roleClaimDescription = null, string? roleClaimGroup = null)
        : base()
    {
        Description = roleClaimDescription;
        Group = roleClaimGroup;
    }
}