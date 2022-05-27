using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.Blog.Models;

internal sealed class BlogModel : IBlog
{
    public long Id
    {
        get;
    }

    public string Title
    {
        get;
        set;
    }

    public string Slug
    {
        get;
        set;
    }

    public IBlogUser Author
    {
        get;
        set;
    }

    public BlogModel(long id, string title, string slug, IBlogUser author)
    {
        Id = id;
        Title = title;
        Slug = slug;
        Author = author;
    }
}