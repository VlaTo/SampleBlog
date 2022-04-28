using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Domain.Entities;

public interface IBlog : IEntity<long>
{
    string Title
    {
        get;
        set;
    }

    string Slug
    {
        get;
        set;
    }

    IBlogUser Author
    {
        get;
        set;
    }
}