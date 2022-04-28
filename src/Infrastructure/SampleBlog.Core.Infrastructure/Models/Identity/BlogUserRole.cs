using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Infrastructure.Models.Identity;

public sealed class BlogUserRole : IdentityRole, IAuditableEntity<string>
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

    public ICollection<BlogUserRoleClaim> RoleClaims
    {
        get;
        set;
    }
    public BlogUserRole()
        : base()
    {
        RoleClaims = new HashSet<BlogUserRoleClaim>();
    }

    public BlogUserRole(string roleName, string? roleDescription = null)
        : base(roleName)
    {
        RoleClaims = new HashSet<BlogUserRoleClaim>();
        Description = roleDescription;
    }
}