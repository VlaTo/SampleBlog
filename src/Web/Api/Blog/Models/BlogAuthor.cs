using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.Blog.Models;

internal sealed class BlogAuthor : IBlogUser
{
    public string Id
    {
        get;
    }

    public bool IsActive
    {
        get;
        set;
    }

    public string UserName
    {
        get;
        set;
    }

    public string RefreshToken
    {
        get;
        set;
    }

    public BlogAuthor(string id)
    {
        Id = id;
    }
}