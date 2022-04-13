using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Domain.Entities;

public sealed class Blog : IEntity<long>
{
    public long Id
    {
        get;
    }

    public Blog(long id)
    {
        Id = id;
    }
}