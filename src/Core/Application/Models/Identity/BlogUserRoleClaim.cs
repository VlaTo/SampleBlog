using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Application.Models.Identity;

public class BlogUserRoleClaim : IdentityRoleClaim<string>, IAuditableEntity<int>
{
    public override int Id
    {
        get;
        set;
    }

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


}