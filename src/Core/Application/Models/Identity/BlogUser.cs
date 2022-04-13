using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Application.Models.Identity;

public sealed class BlogUser : IdentityUser<string>, IAuditableEntity<string>
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

    public BlogUser()
    {
    }
}