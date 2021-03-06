using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Domain.Entities;

public interface IBlogUser : IEntity<string>
{
    bool IsActive
    {
        get;
        set;
    }

    string UserName
    {
        get;
        set;
    }

    string RefreshToken
    {
        get;
        set;
    }
}