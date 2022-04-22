using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Domain.Contracts;
using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Core.Application.Models.Identity;

public sealed class BlogUser : IdentityUser<string>, IBlogUser, IAuditableEntity<string>
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

    public string RefreshToken
    {
        get;
        set;
    }

    public DateTime RefreshTokenExpiryTime
    {
        get;
        set;
    }

    public BlogUser()
    {
    }
}