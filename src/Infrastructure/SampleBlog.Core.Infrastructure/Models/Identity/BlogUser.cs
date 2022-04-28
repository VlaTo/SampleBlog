using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Domain.Contracts;
using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Infrastructure.Models.Identity;

public sealed class BlogUser : IdentityUser, IBlogUser, IAuditableEntity<string>
{
    public bool IsActive
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

    public string? RefreshToken
    {
        get;
        set;
    }

    public DateTime? RefreshTokenExpiryTime
    {
        get;
        set;
    }

    public BlogUser()
        : base()
    {
    }

    public BlogUser(string userName)
        : base(userName)
    {
    }
}